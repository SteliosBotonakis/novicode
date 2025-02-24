namespace NoviCode.WalletService.Strategies;

public class ForceSubtractFundsStrategy : IBalanceStrategy
{
    public void Execute(Data.Wallet wallet, decimal convertedAmount)
    {
        wallet.Balance -= convertedAmount;
    }
}