using System;

namespace Coinbot.SQLite.Models
{
    public class Order
    {
        public string Id { get; set; }
        public string SellId { get; set; }
        public string BuyId { get; set; }
        public string BaseCoin { get; set; }
        public string TargetCoin { get; set; }
        public double ChangeToSell { get; set; }
        public double Stack { get; set; }
        public double BoughtFor { get; set; }
        public double ToSellFor { get; set; }
        public double? SoldFor { get; set; }
        public double QuantityBought { get; set; }
        public double QuantitySold { get; set; }
        public bool SellOrderPlaced { get; set; }
        public bool Sold { get; set; }
        public bool Bought { get; set; }
        public string SessionId { get; set; }
        public DateTime InsertedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Stock { get; set; }
    }
}