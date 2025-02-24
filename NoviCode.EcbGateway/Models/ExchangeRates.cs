namespace NoviCode.EcbGateway.Models
{
    public class ExchangeRates
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}