using System;

namespace Coinbot.SQLite.Models
{
    public class Tick
    {
        public long Id { get; set; }
        public string Stock { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
        public double Last { get; set; }
        public string BaseCoin { get; set; }
        public string TargetCoin { get; set; }
        public string SessionId { get; set; }
        public DateTime InsertedAt { get; set; }

    }
}