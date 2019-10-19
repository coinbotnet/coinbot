using System.Globalization;
using System;
using System.IO;
using System.Threading.Tasks;
using Coinbot.Domain.Contracts;
using Coinbot.Domain.Contracts.Models;
using Coinbot.Domain.Contracts.Models.StockApiService;

namespace Coinbot.Bitdummy
{
    public class StockApiService : IStockApiService
    {
        private int _line = 0;
        private readonly StreamReader sr = new StreamReader("DummyData.csv");
        public StockApiService()
        {
        }

        public Task<ServiceResponse<Transaction>> GetOrder(string baseCoin, string targetCoin, string apiKey, string secret, string orderRefId)
        {
            throw new NotImplementedException();
        }

        public StockInfo GetStockInfo()
        {
            return new StockInfo {
                FillOrKill = true
            };
        }

        public async Task<ServiceResponse<Tick>> GetTicker(string baseCoin, string targetCoin)
        {
            string currentLine = sr.ReadLine();

            if (currentLine != null)
            {
                var price = double.Parse(currentLine.Split(',')[1], CultureInfo.InvariantCulture);
                return new ServiceResponse<Tick>(0, new Tick {
                    Ask = price,
                    Bid = price,
                    Last = price
                });
            }
            else
                return new ServiceResponse<Tick>(100, null, "End of file");
            
        }

        public async Task<ServiceResponse<Transaction>> PlaceBuyOrder(string baseCoin, string targetCoin, double stack, string apiKey, string secret, double rate)
        {
            return new ServiceResponse<Transaction>(0, new Transaction {
                IsOpen = false,
                OrderRefId = Guid.NewGuid().ToString(),
                Quantity = stack / rate
            });
        }

        public async Task<ServiceResponse<Transaction>> PlaceSellOrder(string baseCoin, string targetCoin, double stack, string apiKey, string secret, double qty, double toSellFor, double? raisedChangeToSell = null)
        {
            return new ServiceResponse<Transaction>(0, new Transaction {
                IsOpen = false,
                OrderRefId = Guid.NewGuid().ToString(),
                Quantity = qty
            });
        }
    }
}
