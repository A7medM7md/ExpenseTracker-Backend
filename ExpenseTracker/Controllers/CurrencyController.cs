using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CurrencyController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
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
                    return BadRequest("API key is missing. Please configure it in appsettings.json");
                }

                var url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/EGP";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, $"Error fetching exchange rates: {response.ReasonPhrase}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
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
}
