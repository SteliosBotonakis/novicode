using NoviCode.EcbGateway.Models;

namespace NoviCode.EcbGateway.Services
{
    public interface IEcbGateway
    {
        Task<ExchangeRates> GetLatestRatesAsync(CancellationToken cancellationToken = default);
    }
}