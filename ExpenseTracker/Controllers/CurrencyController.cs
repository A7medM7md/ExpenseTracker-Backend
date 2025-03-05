using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CurrencyController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        [HttpGet("rates")]
        public async Task<IActionResult> GetExchangeRates()
        {
            try
            {
                var apiKey = _configuration["ExchangeRateApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Unauthorized("Invalid API Key. Please check your configuration.");
                }

                var url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/EGP";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, $"Error fetching exchange rates: {response.ReasonPhrase}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var exchangeRates = JsonSerializer.Deserialize<ExchangeRateResponse>(content);
                return Ok(exchangeRates);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Network error: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }

    public class ExchangeRateResponse
    {
        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; }

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; }
    }
}
