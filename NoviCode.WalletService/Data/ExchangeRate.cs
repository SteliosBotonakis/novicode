namespace NoviCode.WalletService.Data;

public class ExchangeRate
{
    // We are using a composite key of Date and Currency.
    public DateTime Date { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}
