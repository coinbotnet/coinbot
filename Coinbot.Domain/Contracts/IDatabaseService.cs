using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coinbot.Domain.Contracts.Models;
using Coinbot.Domain.Contracts.Models.DatabaseService;


namespace Coinbot.Domain.Contracts
{
    public interface IDatabaseService
    {
        Task<ServiceResponse<Order>> SaveTransaction(double boughtFor, string buyId, string baseCoin, string targetCoin, double changeToSell, string sessionId, double stack, string stock);
        Task<ServiceResponse<List<Order>>> GetUnsoldTransactions(string baseCoin, string targetCoin, string stock);
        Task<ServiceResponse<List<Order>>> GetPendingTransactions(string stock);
        Task<ServiceResponse<List<Order>>> GetPendingBuyTransactions(string baseCoin, string targetCoin, string stock);
        Task<ServiceResponse<double>> GetAwaitingSellSum(string baseCoin, string targetCoin, string stock);
        Task<ServiceResponse> TransactionOrderPlaced(string transactionId, string sellId, double? soldFor = null);
        Task<ServiceResponse> TransactionOrderFinalized(string transactionId, double quantity);
        Task<ServiceResponse> TransactionBuyFinalized(string transactionId, double quantity);
        Task<ServiceResponse> SaveTick(string baseCoin, string targetCoin, string stock, double ask, double bid, double last, string sessionId);
        Task<ServiceResponse<double>> GetProfit(string baseCoin, string targetCoin, string stock);
    }
}