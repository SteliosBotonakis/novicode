using System;
using System.Net.Http;
using System.Threading.Tasks;
using NoviCode.EcbGateway.Services;

namespace NoviCode.TestRunner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var httpClient = new HttpClient();
            var ecbGateway = new EcbGateway.Services.EcbGateway(httpClient);

            try
            {
                var exchangeRates = await ecbGateway.GetLatestRatesAsync();
                Console.WriteLine($"Rates Date: {exchangeRates.Date:d}");
                foreach (var rate in exchangeRates.Rates)
                {
                    Console.WriteLine($"{rate.Key}: {rate.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}