namespace NoviCode.WalletService.Strategies;

public interface IStrategyFactory
{
    IBalanceStrategy GetStrategy(string strategyName);
}

public class StrategyFactory : IStrategyFactory
{
    public IBalanceStrategy GetStrategy(string strategyName)
    {
        return strategyName.ToLower() switch
        {
            "addfundsstrategy" => new AddFundsStrategy(),
            "subtractfundsstrategy" => new SubtractFundsStrategy(),
            "forcesubtractfundsstrategy" => new ForceSubtractFundsStrategy(),
            _ => throw new ArgumentException($"Unknown strategy: {strategyName}")
        };
    }
}