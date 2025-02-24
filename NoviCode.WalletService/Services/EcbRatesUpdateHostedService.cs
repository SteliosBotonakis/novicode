using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NoviCode.EcbGateway.Models;
using NoviCode.EcbGateway.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using NoviCode.WalletService.Configuration;

public class EcbRatesUpdateHostedService : IHostedService, IDisposable
{
    private readonly IEcbGateway _ecbGateway;
    // private readonly MyDbContext _dbContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EcbRatesUpdateHostedService> _logger;
    private Timer? _timer;

    public EcbRatesUpdateHostedService(
        IEcbGateway ecbGateway,
        // MyDbContext dbContext,
        IServiceScopeFactory scopeFactory,
        ILogger<EcbRatesUpdateHostedService> logger)
    {
        _ecbGateway = ecbGateway;
        // _dbContext = dbContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Start immediately, then run every minute.
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            // Fetch the latest exchange rates using the gateway from Task 1.
            ExchangeRates latestRates = await _ecbGateway.GetLatestRatesAsync();

            // Build the raw SQL MERGE statement.
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                string mergeSql = BuildMergeSql(latestRates);
                await dbContext.Database.ExecuteSqlRawAsync(mergeSql);
            }

            _logger.LogInformation("Exchange rates updated for {Date}", latestRates.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ECB rates");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private string BuildMergeSql(ExchangeRates rates)
    {
        // Build the MERGE statement that processes all currency rates for the given date.
        // Example MERGE SQL:
        // MERGE INTO [ExchangeRates] AS T
        // USING (VALUES ('2025-02-19','USD',1.0434), ('2025-02-19','JPY',158.41), ...)
        // AS S([Date],[Currency],[Rate])
        // ON (T.[Date] = S.[Date] AND T.[Currency] = S.[Currency])
        // WHEN MATCHED THEN UPDATE SET T.[Rate] = S.[Rate]
        // WHEN NOT MATCHED THEN INSERT ([Date],[Currency],[Rate]) VALUES (S.[Date], S.[Currency], S.[Rate]);

        string dateString = rates.Date.ToString("yyyy-MM-dd");
        string valuesList = string.Join(",\n", rates.Rates.Select(r =>
            $"('{dateString}', '{r.Key}', {r.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)})"));

        string sql = $@"
MERGE INTO [ExchangeRates] AS T
USING (VALUES
  {valuesList}
) AS S([Date],[Currency],[Rate])
ON (T.[Date] = S.[Date] AND T.[Currency] = S.[Currency])
WHEN MATCHED THEN 
    UPDATE SET T.[Rate] = S.[Rate]
WHEN NOT MATCHED THEN
    INSERT ([Date],[Currency],[Rate]) 
    VALUES (S.[Date], S.[Currency], S.[Rate]);
";
        return sql;
    }
}
