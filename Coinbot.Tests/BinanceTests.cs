using System;
using Xunit;
using Coinbot.Binance;
using Coinbot.Domain.Contracts;
using Coinbot.Domain.Contracts.Models;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using System.Reflection;

namespace Coinbot.Tests
{
    [Trait("Category", "Binance")]
    public class BinanceTests
    {
        private readonly IStockApiService _service;
        private readonly ServiceProvider _provider;

        public BinanceTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddAutoMapper(typeof(StockApiService))
                .AddTransient<IStockApiService, StockApiService>();

            _provider = serviceCollection.BuildServiceProvider();
            _service = _provider.GetService<IStockApiService>();
        }

        [Fact]
        public async void GetTick()
        {
            var result = await _service.GetTicker("BTC","LTC");
            Assert.True(result.Success, "Couldn't get ticker from Binance Exchange API");
        }

        [Fact]
        public async void GetOrder()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _service.GetOrder("BTC","LTC", System.Environment.GetEnvironmentVariable("API_KEY"), System.Environment.GetEnvironmentVariable("SECRET"), "1"));
            Assert.Equal("Binance GetOrder method returned: BadRequest Msg: {\"code\":-2013,\"msg\":\"Order does not exist.\"}", exception.Message);
        }
    }
}
