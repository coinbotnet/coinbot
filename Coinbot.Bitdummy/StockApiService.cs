using System.Globalization;
using System;
using System.IO;
using System.Threading.Tasks;
using Coinbot.Domain.Contracts;
using Coinbot.Domain.Contracts.Models;
using Coinbot.Domain.Contracts.Models.StockApiService;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;

namespace Coinbot.Bitdummy
{
    public class StockApiService : IStockApiService
    {
        private readonly string _serviceUrl = "https://api.coincap.io/v2/";
        private readonly HttpClient _client = new HttpClient();
        private int _line = 0;
        private CoinCapAssets _assets = null;
        private CoinCapRates _rates = null;
        private CoinCapMarket _market = null;
        private string _interval = "m5";
        private int _index = 0;
        public StockApiService(SessionInfo session)
        {
            _client.BaseAddress = new Uri(_serviceUrl);

            switch (session.Interval)
            {
                case 60:
                    _interval = "m1";
                    break;
                case 300:
                    _interval = "m5";
                    break;
                case 900:
                    _interval = "m15";
                    break;
                case 1800:
                    _interval = "m30";
                    break;
                case 3600:
                    _interval = "h1";
                    break;
                case 7200:
                    _interval = "h2";
                    break;
                case 21600:
                    _interval = "h6";
                    break;
                case 86400:
                    _interval = "d1";
                    break;
            }

            try
            {
                HttpResponseMessage responseRates = _client.GetAsync("rates?type=crypto").Result;
                responseRates.EnsureSuccessStatusCode();
                _rates = JsonConvert.DeserializeObject<CoinCapRates>(responseRates.Content.ReadAsStringAsync().Result);

                HttpResponseMessage responseAssets = _client.GetAsync($"assets?search={session.BaseCoin}&limit=1").Result;
                responseAssets.EnsureSuccessStatusCode();
                _assets = JsonConvert.DeserializeObject<CoinCapAssets>(responseAssets.Content.ReadAsStringAsync().Result);

                HttpResponseMessage responseMarket = _client.GetAsync($"assets/{_assets.data.FirstOrDefault().id}/history?interval={_interval}").Result;
                responseAssets.EnsureSuccessStatusCode();
                _market = JsonConvert.DeserializeObject<CoinCapMarket>(responseMarket.Content.ReadAsStringAsync().Result);

                foreach (var item in _market.data)
                {
                    item.priceQuote = _rates.data.FirstOrDefault(x => x.symbol == session.TargetCoin).rateUsd / item.priceUsd;
                }

            }
            catch
            {
                throw new Exception("Could not get historical data from CoinCap.io");
            }
        }

        public Task<ServiceResponse<Transaction>> GetOrder(string baseCoin, string targetCoin, string apiKey, string secret, string orderRefId)
        {
            throw new NotImplementedException();
        }

        public StockInfo GetStockInfo()
        {
            return new StockInfo
            {
                FillOrKill = true
            };
        }

        public async Task<ServiceResponse<Tick>> GetTicker(string baseCoin, string targetCoin)
        {

            try
            {
                _index++;
                return new ServiceResponse<Tick>(0, new Tick
                {
                    Ask = _market.data[_index - 1].priceQuote,
                    Bid = _market.data[_index - 1].priceQuote,
                    Last = _market.data[_index - 1].priceQuote
                });
            }
            catch
            {
                return new ServiceResponse<Tick>(100, null, "End of data");
            }

        }

        public async Task<ServiceResponse<Transaction>> PlaceBuyOrder(string baseCoin, string targetCoin, double stack, string apiKey, string secret, double rate, bool? testOnly = false)
        {
            return new ServiceResponse<Transaction>(0, new Transaction
            {
                IsOpen = false,
                OrderRefId = Guid.NewGuid().ToString(),
                Quantity = stack / rate
            });
        }

        public async Task<ServiceResponse<Transaction>> PlaceSellOrder(string baseCoin, string targetCoin, double stack, string apiKey, string secret, double qty, double toSellFor, double? raisedChangeToSell = null, bool? testOnly = false)
        {
            return new ServiceResponse<Transaction>(0, new Transaction
            {
                IsOpen = false,
                OrderRefId = Guid.NewGuid().ToString(),
                Quantity = qty
            });
        }
    }
}
