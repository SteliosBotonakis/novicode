namespace NoviCode.WalletService.Strategies;

public class SubtractFundsStrategy : IBalanceStrategy
{
    public void Execute(Data.Wallet wallet, decimal convertedAmount)
    {
        if (wallet.Balance < convertedAmount)
            throw new InvalidOperationException("Insufficient funds.");
        wallet.Balance -= convertedAmount;
    }
}