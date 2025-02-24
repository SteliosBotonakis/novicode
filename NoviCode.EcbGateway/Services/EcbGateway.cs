using System.Globalization;
using System.Xml.Linq;
using NoviCode.EcbGateway.Models;

namespace NoviCode.EcbGateway.Services
{
    public class EcbGateway : IEcbGateway
    {
        private const string EcbUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
        private readonly HttpClient _httpClient;

        public EcbGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExchangeRates> GetLatestRatesAsync(CancellationToken cancellationToken = default)
        {
            // 1) Fetch XML from ECB
            var response = await _httpClient.GetAsync(EcbUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            // 2) Parse the XML content
            var xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseExchangeRates(xmlContent);
        }

        private ExchangeRates ParseExchangeRates(string xml)
        {

            var xDoc = XDocument.Parse(xml);

            // Find the <Cube> that has the "time" attribute
            var cubeTimeElement = xDoc.Descendants().FirstOrDefault(e => e.Attribute("time") != null);
            if (cubeTimeElement == null)
                throw new InvalidOperationException("Unable to find the Cube element with time attribute in ECB XML.");

            var dateAttribute = cubeTimeElement.Attribute("time")?.Value;
            if (dateAttribute == null || !DateTime.TryParse(dateAttribute, out var date))
                throw new InvalidOperationException("Invalid or missing date in ECB XML.");

            // Extract currency-rate pairs
            var rates = cubeTimeElement.Elements()
                .Select(e => new
                {
                    Currency = e.Attribute("currency")?.Value,
                    Rate = e.Attribute("rate")?.Value
                })
                .Where(x => x.Currency != null && x.Rate != null)
                .ToDictionary(
                    x => x.Currency!,
                    x => decimal.Parse(x.Rate!, CultureInfo.InvariantCulture)
                );

            return new ExchangeRates
            {
                Date = date,
                Rates = rates
            };
        }
    }
}
