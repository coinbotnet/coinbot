using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coinbot.Domain.Contracts.Models;
using Coinbot.Domain.Contracts.Models.StockApiService;


namespace Coinbot.Domain.Contracts
{
    public interface IStockApiService
    {
        StockInfo GetStockInfo();
        Task<ServiceResponse<Tick>> GetTicker(string baseCoin, string targetCoin);

        Task<ServiceResponse<Transaction>> PlaceSellOrder(string baseCoin, string targetCoin, double stack, string apiKey, string secret, double qty, double toSellFor, double? raisedChangeToSell = null);

        Task<ServiceResponse<Transaction>> PlaceBuyOrder(string baseCoin, string targetCoin, double stack, string apiKey, string secret, double rate);

        Task<ServiceResponse<Transaction>> GetOrder(string baseCoin, string targetCoin, string apiKey, string secret, string orderRefId);
    }
}