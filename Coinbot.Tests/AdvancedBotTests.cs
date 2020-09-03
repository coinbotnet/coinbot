using System.Linq;
using System;
using AutoMapper;
using Coinbot.Bitdummy;
using Coinbot.Core.Implementations;
using Coinbot.Domain.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Coinbot.Domain.Contracts.Models;

namespace Coinbot.Tests
{
    [Trait("Category", "AdvancedBot")]
    public class AdvancedBotTests
    {
        private readonly IBot _bot;
        private readonly ServiceProvider _provider;
        private readonly IDatabaseService _db;
        private readonly ILogger<AdvancedBotTests> _logger;
        private readonly SessionInfo _session;
        public AdvancedBotTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                })
                .AddAutoMapper(typeof(StockApiService))
                .AddTransient<IStockApiService, StockApiService>()
                .AddTransient<IBot, AdvancedBot>()
                .AddSingleton<IDatabaseService, TempDb.DatabaseService>()
                .AddSingleton<SessionInfo>(new SessionInfo
                {
                    BuyoutCeiling = 5000,
                    ChangeToBuy = 0.05,
                    ChangeToSell = 0.05,
                    Interval = 30,
                    BaseCoin = "PLN",
                    TargetCoin = "BTC",
                    Stock = "Bitbay",
                    Stack = 250
                });

            _provider = serviceCollection.BuildServiceProvider();
            _bot = _provider.GetService<IBot>();
            _db = _provider.GetService<IDatabaseService>();
            _logger = _provider.GetService<ILogger<AdvancedBotTests>>();
            _session = _provider.GetService<SessionInfo>();

        }

        [Fact]
        public async void Simulation()
        {
            ServiceResponse resp = new ServiceResponse(0, "OK");
            
            while(resp.Success)
            {
                resp = await _bot.BuyIfConditionsMet();
                await _bot.SellIfConditionsMet();
            }

            var profit = await _db.GetProfit(_session.BaseCoin, _session.TargetCoin, _session.Stock);
            var remaining = await _db.GetUnsoldTransactions(_session.BaseCoin, _session.TargetCoin, _session.Stock);

            _logger.LogInformation($"Bot has made {profit.Data}");
            _logger.LogInformation($"Bot didn't manage to sell a remaining {remaining.Data.Sum(x=> x.QuantityBought)} BTC");

        }
    }
}