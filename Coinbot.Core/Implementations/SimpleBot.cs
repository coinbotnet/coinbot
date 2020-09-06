using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Coinbot.Domain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Options;
using Coinbot.Domain.Contracts.Models;

namespace Coinbot.Core.Implementations
{
    public class SimpleBot : IBot
    {
        private readonly IMapper _mapper;
        private readonly IStockApiService _service;
        private readonly IDatabaseService _db;
        private readonly ILogger<SimpleBot> _logger;
        private readonly SessionInfo _session;
        private Stack<double> _stack = new Stack<double>();
        private Stack<double> _previousPercentage = new Stack<double>();
        private Dictionary<string, double> _previousSellPercentage = new Dictionary<string, double>();
        private bool _huntingModeActive = false;
        public SimpleBot(IMapper mapper,
                    IStockApiService service,
                    IDatabaseService db,
                    ILogger<SimpleBot> logger,
                    SessionInfo session)
        {
            _service = service;
            _db = db;
            _logger = logger;
            _session = session;
            _mapper = mapper;
        }

        private void ClearStacks()
        {
            _stack.Clear();
            _previousPercentage.Clear();
            _huntingModeActive = false;
        }

        public async Task<ServiceResponse> BuyIfConditionsMet()
        {
            // we need to know how much the bot has bought till now so it won't exceed the limit
            var amountBought = await _db.GetAwaitingSellSum(_session.BaseCoin, _session.TargetCoin, _session.Stock);
            var result = await _service.GetTicker(_session.BaseCoin, _session.TargetCoin);

            if (result.Success && amountBought.Success)
            {
                var saveTickResult = await _db.SaveTick(_session.BaseCoin, _session.TargetCoin, _session.Stock, result.Data.Ask, result.Data.Bid, result.Data.Last, _session.Id);
                _stack.Push(result.Data.Ask);
                var percentageChange = 1 - (_stack.Peek() / _stack.Max());
                _previousPercentage.Push(percentageChange);

                _logger.LogTrace("Max of stack: {0} Tick: {1}", _stack.Max().ToString("0.00000000", CultureInfo.InvariantCulture), _stack.Peek().ToString("0.00000000", CultureInfo.InvariantCulture));
                _logger.LogTrace($"Percentage change: {_previousPercentage.Peek()}. Maximum percentage change: {_previousPercentage.Max()}");

                // here we check if the next transaction would exceed the limit
                if (amountBought.Data + _session.Stack > _session.BuyoutCeiling)
                {
                    // if it exceeds the limit we should start from the beginning
                    ClearStacks();
                }
                else
                {
                    if (!_huntingModeActive && (percentageChange > _session.ChangeToBuy))
                    {
                        // hunting mode has been triggered when exceeded change to buy parameter. Now it will try to buy as low as it can
                        _huntingModeActive = true;
                    }

                    if (_previousPercentage.Count >= 2 && _huntingModeActive)
                    {
                        var percents = _previousPercentage.Take(2).ToArray();

                        if (percents[0] < percents[1])
                        {
                            // if the actual percentage is 0 it means the rate has reached max in stack. This means we shouldn't buy any coins at this price, and yet we start from the beginning from the top of the chart
                            if (percents[0] == 0)
                            {
                                ClearStacks();
                                return new ServiceResponse(0, "The rate has reached max in stack");
                            }
                            
                            var serviceResult = await _service.PlaceBuyOrder(_session.BaseCoin, _session.TargetCoin, _session.Stack, _session.ApiKey, _session.Secret, _stack.Peek(), _session.TestMode);

                            if (serviceResult.Success)
                            {
                                var dbResult = await _db.SaveTransaction(_stack.Peek(), serviceResult.Data.OrderRefId, _session.BaseCoin, _session.TargetCoin, _session.ChangeToSell, _session.Id, _session.Stack, _session.Stock);

                                if (dbResult.Success)
                                {
                                    _logger.LogInformation(string.Format("Placed {0} buy order with id {2} for {1}", _session.TargetCoin, _stack.Peek().ToString("0.00000000"),serviceResult.Data.OrderRefId));
                                    ClearStacks();

                                    if (_service.GetStockInfo().FillOrKill)
                                    {
                                        _logger.LogInformation(string.Format("Buy order id {0} finalized. Bought {1}", serviceResult.Data.OrderRefId, serviceResult.Data.Quantity));
                                        await _db.TransactionBuyFinalized(dbResult.Data.Id, serviceResult.Data.Quantity);
                                    }
                                }
                            }
                            else
                            {
                                // probably something went wrong - no funds on exchange or problem with api or fillOrKill enabled. let's clear percentage so it won't make false assumptions. It may try to buy some coins after a while when the change is not profitable anymore
                                ClearStacks();

                                return new ServiceResponse(0, "Fill or Kill couldn't complete an order or there was problem contacting StockApi");
                            }
                        }
                    }
                }
            }
            else
                return new ServiceResponse(100, "Couldn't get any info about orders or ticker");

            return new ServiceResponse(0, "Conditions didn't met yet. Waiting ...");
        }

        public async Task<ServiceResponse> CheckIfBought()
        {
            var result = await _db.GetPendingBuyTransactions(_session.BaseCoin, _session.TargetCoin, _session.Stock);

            if (result.Success)
            {
                foreach (var item in result.Data)
                {
                    var serviceResult = await _service.GetOrder(_session.BaseCoin, _session.TargetCoin, _session.ApiKey, _session.Secret, item.BuyId);

                    if (serviceResult.Success)
                    {
                        if (!serviceResult.Data.IsOpen)
                        {
                            _logger.LogInformation(string.Format("Buy order id {0} finalized. Bought {1}", serviceResult.Data.OrderRefId, serviceResult.Data.Quantity));
                            await _db.TransactionBuyFinalized(item.Id, serviceResult.Data.Quantity);
                        }
                    }
                    return new ServiceResponse(serviceResult.Status, serviceResult.Message);
                }
            }
            return new ServiceResponse(result.Status, result.Message);
        }

        public async Task<ServiceResponse> CheckIfSold()
        {

            var result = await _db.GetPendingTransactions(_session.Stock);

            if (result.Success)
            {
                foreach (var item in result.Data)
                {
                    var serviceResult = await _service.GetOrder(_session.BaseCoin, _session.TargetCoin, _session.ApiKey, _session.Secret, item.SellId);

                    if (serviceResult.Success)
                    {
                        if (!serviceResult.Data.IsOpen)
                        {
                            _logger.LogInformation(string.Format("Sell order id {0} finalized. Sold {1}", serviceResult.Data.OrderRefId, serviceResult.Data.Quantity));
                            await _db.TransactionOrderFinalized(item.Id, serviceResult.Data.Quantity);
                        }
                    }
                    return new ServiceResponse(serviceResult.Status, serviceResult.Message);
                }
            }
            return new ServiceResponse(result.Status, result.Message);
        }

        public async Task<ServiceResponse> SellIfConditionsMet()
        {
            // find every unsold transaction in db. unsold means it has been bought by the bot
            var result = await _db.GetUnsoldTransactions(_session.BaseCoin, _session.TargetCoin, _session.Stock);

            if (result.Success)
            {
                double greedyTick = 0;


                var tickResult = await _service.GetTicker(_session.BaseCoin, _session.TargetCoin);
                if (tickResult.Success)
                    greedyTick = tickResult.Data.Last;


                foreach (var item in result.Data)
                {
                    double? greedyRate = null;

                    // if tick is 
                    if (greedyTick >= item.ToSellFor)
                    {
                        var percentageChange = 1 - (item.ToSellFor / greedyTick);

                        if (!_previousSellPercentage.ContainsKey(item.Id))
                        {
                            _previousSellPercentage[item.Id] = percentageChange;
                            continue;
                        }

                        if (Math.Round(_previousSellPercentage[item.Id], 3) <= Math.Round(percentageChange, 3))
                        {
                            _previousSellPercentage[item.Id] = percentageChange;
                            continue;
                        }
                        else
                            greedyRate = greedyTick;
                    }
                    else
                        continue;


                    var serviceResult = await _service.PlaceSellOrder(_session.BaseCoin, _session.TargetCoin, _session.Stack, _session.ApiKey, _session.Secret, item.QuantityBought, item.ToSellFor, greedyRate, _session.TestMode);

                    if (serviceResult.Success)
                    {
                        _logger.LogInformation(string.Format("Placed new sell order with id {0} for {1}", serviceResult.Data.OrderRefId, greedyRate));
                        var dbResult = await _db.TransactionOrderPlaced(item.Id, serviceResult.Data.OrderRefId, greedyRate);

                        if (dbResult.Success)
                        {
                            if (_service.GetStockInfo().FillOrKill)
                            {
                                _logger.LogInformation(string.Format("Sell order id {0} finalized. Sold {1}", serviceResult.Data.OrderRefId, serviceResult.Data.Quantity));
                                await _db.TransactionOrderFinalized(item.Id, serviceResult.Data.Quantity);
                            }
                        }
                        // we dont need to clear _previousSellPercentage[item.Id] as this wont be updated anymore. the item is considered sold and won't enter this code block

                        var profit = await _db.GetProfit(_session.BaseCoin, _session.TargetCoin, _session.Stock);
                        
                        if(profit.Success)
                            _logger.LogInformation($"Simple bot earned {profit.Data.ToString("0.00000000")} {_session.BaseCoin} so far");
                    }
                    return new ServiceResponse(serviceResult.Status, serviceResult.Message);
                }
            }
            return new ServiceResponse(result.Status, result.Message);
        }
    }
}