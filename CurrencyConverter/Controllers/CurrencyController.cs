using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyController : Controller
    {
        private readonly ILogger<CurrencyController> _logger;
        private readonly Dictionary<string, decimal> exchangeRates;
        public CurrencyController(ILogger<CurrencyController> logger)
        {
            _logger = logger;
            try
            {
                // Initialize exchange rates from the local JSON file
                // Initialize exchange rates from the local JSON file
                string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ExchangeRates.json");
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                exchangeRates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(jsonContent);

                // Override exchange rates with values from environment variables if set
                foreach (var (key, rate) in exchangeRates.ToList())
                {
                    if (TryGetOverrideRate(key, out decimal overrideRate))
                    {
                        exchangeRates[key] = overrideRate;
                    }
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"Exception occured while loading the exchange rates. {ex}");
            }

        }

        private bool TryGetOverrideRate(string key, out decimal overrideRate)
        {
            string envVar = Environment.GetEnvironmentVariable(key);
            return decimal.TryParse(envVar, out overrideRate);
        }

        [HttpPost("convert")]
        public ActionResult<ConversionResult> ConvertCurrency([FromBody] ConversionRequest request)
        {
            try
            {
                // Create a key for the requested currency pair
                string key = $"{request.SourceCurrency}_TO_{request.TargetCurrency}";

                // Check if the specified currency pair is supported
                if (!exchangeRates.ContainsKey(key))
                {
                    _logger.LogError($"Unsupported currency pair. SourceCurrency {request.SourceCurrency} TargetCurrency {request.TargetCurrency}");
                    return BadRequest("Unsupported currency pair");
                }

                // Get the exchange rate from the loaded rates
                decimal exchangeRate = exchangeRates[key];

                // Perform the currency conversion
                decimal convertedAmount = request.Amount * exchangeRate;

                _logger.LogInformation($"Successfully converted the currency from SourceCurrency {request.SourceCurrency} TargetCurrency {request.TargetCurrency} for amount {request.Amount}. ConvertedAmount {convertedAmount}");
                // Return the result in JSON format
                return new ConversionResult
                {
                    ExchangeRate = exchangeRate,
                    ConvertedAmount = convertedAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred during Currency Conversion : {ex}");
          
            }
            return NotFound("Oops! something went wrong during conversion...");
        }
    }
}
