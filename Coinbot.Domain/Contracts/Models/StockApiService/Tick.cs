namespace Coinbot.Domain.Contracts.Models.StockApiService
{
    public class Tick
    {
        public double Last { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
    }
}