namespace NoviCode.WalletService.Strategies;

public interface IBalanceStrategy
{
    void Execute(Data.Wallet wallet, decimal convertedAmount);
}