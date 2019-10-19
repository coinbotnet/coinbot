namespace Coinbot.Domain.Contracts.Models.StockApiService
{
    public class Transaction
    {
        public string OrderRefId { get; set; }
        public double Quantity { get; set; }
        public bool IsOpen { get; set; }
    }
}