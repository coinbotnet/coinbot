using System;
using System.Collections.Generic;

namespace Coinbot.Bitdummy
{
    public class CoinCapMarketElement
    {
        public double priceUsd { get; set; }
        public double priceQuote {get;set;}
        public object time { get; set; }
        public string circulatingSupply { get; set; }
        public DateTime date { get; set; }
    }

    public class CoinCapMarket
    {
        public List<CoinCapMarketElement> data { get; set; }
        public long timestamp { get; set; }
    }
}