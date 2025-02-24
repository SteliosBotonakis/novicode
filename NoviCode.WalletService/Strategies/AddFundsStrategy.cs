namespace NoviCode.WalletService.Strategies;

public class AddFundsStrategy : IBalanceStrategy
{
    public void Execute(Data.Wallet wallet, decimal convertedAmount)
    {
        wallet.Balance += convertedAmount;
    }
}