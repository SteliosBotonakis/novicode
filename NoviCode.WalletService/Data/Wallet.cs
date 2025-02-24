namespace NoviCode.WalletService.Data
{
    public class Wallet
    {
        public long Id { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "EUR"; // Default currency, or set as needed.
    }
}