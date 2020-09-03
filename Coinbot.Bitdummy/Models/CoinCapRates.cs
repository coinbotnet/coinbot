using System.Collections.Generic;

namespace Coinbot.Bitdummy
{
    public class CoinCapRatesElement
    {
        public string id { get; set; }
        public string symbol { get; set; }
        public string currencySymbol { get; set; }
        public string type { get; set; }
        public double rateUsd { get; set; }
    }

    public class CoinCapRates
    {
        public List<CoinCapRatesElement> data { get; set; }
        public long timestamp { get; set; }
    }
}