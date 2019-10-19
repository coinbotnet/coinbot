using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coinbot.Domain.Contracts;
using Coinbot.Domain.Contracts.Models;
using Coinbot.Domain.Contracts.Models.DatabaseService;

namespace Coinbot.TempDb
{
    public class DatabaseService : IDatabaseService
    {
        private object _lock = new object();
        private List<Order> Orders = new List<Order>();

        public async Task<ServiceResponse<double>> GetAwaitingSellSum(string baseCoin, string targetCoin, string stock)
        {
            lock (_lock)
            {
                var result = Orders.Where(x => x.BaseCoin == baseCoin
                                       && x.TargetCoin == targetCoin
                                       && x.Stock == stock.ToLowerInvariant()
                                       && !x.Sold
                                       && x.Bought).ToList();

                return new ServiceResponse<double>(0, result.Sum(x => x.Stack));
            }
        }

        public async Task<ServiceResponse<List<Order>>> GetPendingBuyTransactions(string baseCoin, string targetCoin, string stock)
        {
            lock (_lock)
            {
                var elements = Orders.Where(x => x.BaseCoin == baseCoin
                                        && x.TargetCoin == targetCoin
                                        && x.Stock == stock.ToLowerInvariant()
                                        && !x.Bought)
                                        .ToList();

                if (elements.Count > 0)
                    return new ServiceResponse<List<Order>>(0, elements);
                else
                    return new ServiceResponse<List<Order>>(-2, null);
            }
        }

        public async Task<ServiceResponse<List<Order>>> GetPendingTransactions(string stock)
        {
            lock (_lock)
            {
                var elements = Orders.Where(x => x.Stock == stock.ToLowerInvariant()
                                                        && x.SellOrderPlaced
                                                        && !x.Sold
                                                        && x.Bought)
                                                        .ToList();

                if (elements.Count > 0)
                    return new ServiceResponse<List<Order>>(0, elements);
                else
                    return new ServiceResponse<List<Order>>(-2, null);
            }
        }

        public async Task<ServiceResponse<List<Order>>> GetUnsoldTransactions(string baseCoin, string targetCoin, string stock)
        {
            lock (_lock)
            {
                var elements = Orders.Where(x => x.BaseCoin == baseCoin
                                        && x.TargetCoin == targetCoin
                                        && x.Stock == stock.ToLowerInvariant()
                                        && !x.SellOrderPlaced
                                        && x.Bought)
                                        .ToList();

                if (elements.Count > 0)
                    return new ServiceResponse<List<Order>>(0, elements);
                else
                    return new ServiceResponse<List<Order>>(-2, null);
            }
        }

        public async Task<ServiceResponse> SaveTick(string baseCoin, string targetCoin, string stock, double ask, double bid, double last, string sessionId)
        {
            return new ServiceResponse(0, string.Empty);
        }

        public async Task<ServiceResponse<Order>> SaveTransaction(double boughtFor, string buyId, string baseCoin, string targetCoin, double changeToSell, string sessionId, double stack, string stock)
        {
            lock (_lock)
            {
                var order = new Order
                {
                    BuyId = buyId,
                    Id = Guid.NewGuid().ToString(),
                    BaseCoin = baseCoin,
                    TargetCoin = targetCoin,
                    BoughtFor = boughtFor,
                    ChangeToSell = changeToSell,
                    ToSellFor = boughtFor * (changeToSell + 1),
                    InsertedAt = DateTime.Now,
                    SessionId = sessionId,
                    Stack = stack,
                    Stock = stock.ToLowerInvariant(),
                    Bought = false
                };

                Orders.Add(order);

                return new ServiceResponse<Order>(0, order);
            }
        }

        public async Task<ServiceResponse> TransactionBuyFinalized(string transactionId, double quantity)
        {
            lock (_lock)
            {
                var item = Orders.FirstOrDefault(x => x.Id == transactionId);

                item.QuantityBought = quantity;
                item.Bought = true;
                item.UpdatedAt = DateTime.Now;

                return new ServiceResponse(0);
            }
        }

        public async Task<ServiceResponse> TransactionOrderFinalized(string transactionId, double quantity)
        {
            lock (_lock)
            {
                var item = Orders.FirstOrDefault(x => x.Id == transactionId);

                item.Sold = true;
                item.UpdatedAt = DateTime.Now;
                item.QuantitySold = quantity;


                return new ServiceResponse(0);
            }
        }

        public async Task<ServiceResponse> TransactionOrderPlaced(string transactionId, string sellId, double? soldFor = null)
        {
            lock (_lock)
            {
                var item = Orders.FirstOrDefault(x => x.Id == transactionId);

                item.SellOrderPlaced = true;
                item.UpdatedAt = DateTime.Now;
                item.SellId = sellId;
                item.SoldFor = soldFor != null ? soldFor : item.ToSellFor;


                return new ServiceResponse(0);
            }
        }

        public async Task<ServiceResponse<double>> GetProfit(string baseCoin, string targetCoin, string stock)
        {
            lock (_lock)
            {
                var result = Orders.Where(x => x.BaseCoin == baseCoin
                                        && x.TargetCoin == targetCoin
                                        && x.Stock == stock.ToLowerInvariant()
                                        && x.Sold
                                        && x.Bought).ToList();

                return new ServiceResponse<double>(0, result.Sum(x => (x.SoldFor.GetValueOrDefault() - x.BoughtFor) * x.QuantityBought));
            }
        }
    }
}
