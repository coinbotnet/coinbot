using System;

namespace Coinbot.Core.Models
{
    /// <summary>
    /// Singleton holding settings found from commandline arguments
    /// </summary>
    public sealed class SessionInfo
    {
        private string _id { get; set; }
        public string Id
        {
            private set
            {
                _id = value;
            }
            get
            {
                if (string.IsNullOrWhiteSpace(_id)) { _id = Guid.NewGuid().ToString();}
                return _id;
            }
        }
        public bool OnlySell { get; set; }
        public bool TestMode { get; set; }
        public string BaseCoin { get;  set; }
        public string TargetCoin { get; set; }
        public double ChangeToSell { get;  set; }
        public double ChangeToBuy { get;  set; }

        /// <summary>
        /// How much is the stack ? Base coin considered
        /// </summary>
        public double Stack { get;  set; }
        /// <summary>
        /// How often in seconds you check the stock for new data and also how often a new buyout can occur
        /// </summary>
        public int Interval { get;  set; }
        public string ApiKey { get;  set; }
        public string Secret { get; set; }
        public double BuyoutCeiling { get; set; }
        public string Stock { get; set; }
    }
}