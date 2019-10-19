using System.Collections.Generic;
using System.Threading.Tasks;
using Coinbot.Domain.Contracts;
using Coinbot.Domain.Contracts.Models;
using DbOrder = Coinbot.Domain.Contracts.Models.DatabaseService.Order;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Coinbot.SQLite.Models;
using System;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Options;

namespace Coinbot.SQLite.Implementations
{
    public class DatabaseService : IDatabaseService
    {
        //private readonly CoinbotContext _context;
        private static string _dbPath;
        public IMapper _mapper { get; }

        public DatabaseService(IMapper mapper, IOptionsMonitor<CoinbotConfig> config)
        {
            if (!Directory.Exists(config.CurrentValue.DataPath))
                Directory.CreateDirectory(config.CurrentValue.DataPath);

            _dbPath = Path.Combine(config.CurrentValue.DataPath, config.CurrentValue.DbName);
            _mapper = mapper;

            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                db.Database.Migrate();
            }
        }
        
        public async Task<ServiceResponse> SaveTick(string baseCoin, string targetCoin, string stock, double ask, double bid, double last, string sessionId)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var result = db.Ticks.Add(new Tick{
                    Ask = ask,
                    Bid = bid,
                    Last = last,
                    InsertedAt = DateTime.Now,
                    Stock = stock.ToLowerInvariant(),
                    BaseCoin = baseCoin,
                    TargetCoin = targetCoin,
                    SessionId = sessionId
                });

                await db.SaveChangesAsync();

                return new ServiceResponse(0, "Saved tick to database");
            }
        }

        public async Task<ServiceResponse<double>> GetAwaitingSellSum(string baseCoin, string targetCoin, string stock)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var result = db.Orders.Where(x => x.BaseCoin == baseCoin
                                    && x.TargetCoin == targetCoin
                                    && x.Stock == stock.ToLowerInvariant()
                                    && !x.Sold
                                    && x.Bought).ToList();

                return new ServiceResponse<double>(0, result.Sum(x => x.Stack), "Awaiting sell sum returned");
            }
        }

        public async Task<ServiceResponse<List<DbOrder>>> GetPendingBuyTransactions(string baseCoin, string targetCoin, string stock)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var elements = db.Orders.Where(x => x.BaseCoin == baseCoin
                                    && x.TargetCoin == targetCoin
                                    && x.Stock == stock.ToLowerInvariant()
                                    && !x.Bought)
                                    .Select(x => _mapper.Map<DbOrder>(x)).ToList();

                if (elements.Count > 0)
                    return new ServiceResponse<List<DbOrder>>(0, elements, "Pending buy transactions returned successfully");
                else
                    return new ServiceResponse<List<DbOrder>>(-2, null, "There are no pending buy transations");
            }
        }

        public async Task<ServiceResponse<List<DbOrder>>> GetPendingTransactions(string stock)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var elements = db.Orders.Where(x => x.Stock == stock.ToLowerInvariant()
                                                    && x.SellOrderPlaced
                                                    && !x.Sold
                                                    && x.Bought)
                                                    .Select(x => _mapper.Map<DbOrder>(x)).ToList();

                if (elements.Count > 0)
                    return new ServiceResponse<List<DbOrder>>(0, elements, "Pending sell transactions returned successfully");
                else
                    return new ServiceResponse<List<DbOrder>>(-2, null, "There are no pending sell transations");
            }


        }

        public async Task<ServiceResponse<List<DbOrder>>> GetUnsoldTransactions(string baseCoin, string targetCoin, string stock)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var elements = db.Orders.Where(x => x.BaseCoin == baseCoin
                                    && x.TargetCoin == targetCoin
                                    && x.Stock == stock.ToLowerInvariant()
                                    && !x.SellOrderPlaced
                                    && x.Bought)
                                    .Select(x => _mapper.Map<DbOrder>(x)).ToList();

                if (elements.Count > 0)
                    return new ServiceResponse<List<DbOrder>>(0, elements, "Unsold transactions returned successfully");
                else
                    return new ServiceResponse<List<DbOrder>>(-2, null, "There are no unsold sell transations");
            }
        }

        public async Task<ServiceResponse<DbOrder>> SaveTransaction(double boughtFor, string buyId, string baseCoin, string targetCoin, double changeToSell, string sessionId, double stack, string stock)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
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

                db.Orders.Add(order);

                await db.SaveChangesAsync();

                return new ServiceResponse<DbOrder>(0, _mapper.Map<DbOrder>(order), "Transaction saved to database");
            }

        }

        public async Task<ServiceResponse> TransactionBuyFinalized(string transactionId, double quantity)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var item = db.Orders.FirstOrDefault(x => x.Id == transactionId);

                item.QuantityBought = quantity;
                item.Bought = true;
                item.UpdatedAt = DateTime.Now;

                await db.SaveChangesAsync();

                return new ServiceResponse(0, "Buy transaction altered in database");
            }
        }

        public async Task<ServiceResponse> TransactionOrderFinalized(string transactionId, double quantity)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var item = db.Orders.FirstOrDefault(x => x.Id == transactionId);

                item.Sold = true;
                item.UpdatedAt = DateTime.Now;
                item.QuantitySold = quantity;

                await db.SaveChangesAsync();

                return new ServiceResponse(0, "Sell transaction altered in database");
            }

        }

        public async Task<ServiceResponse> TransactionOrderPlaced(string transactionId, string sellId, double? soldFor = null)
        {
            using (var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var item = db.Orders.FirstOrDefault(x => x.Id == transactionId);

                item.SellOrderPlaced = true;
                item.UpdatedAt = DateTime.Now;
                item.SellId = sellId;
                item.SoldFor = soldFor != null ? soldFor : item.ToSellFor;

                await db.SaveChangesAsync();

                return new ServiceResponse(0, "Transaction written to database");
            }

        }

        public async Task<ServiceResponse<double>> GetProfit(string baseCoin, string targetCoin, string stock)
        {
            using(var db = new CoinbotContext($"Data Source={_dbPath}"))
            {
                var result = db.Orders.Where(x => x.BaseCoin == baseCoin
                                        && x.TargetCoin == targetCoin
                                        && x.Stock == stock.ToLowerInvariant()
                                        && x.Sold
                                        && x.Bought).ToList();

                return new ServiceResponse<double>(0, result.Sum(x => (x.SoldFor.GetValueOrDefault() - x.BoughtFor) * x.QuantityBought));
            }
        }
    }
}