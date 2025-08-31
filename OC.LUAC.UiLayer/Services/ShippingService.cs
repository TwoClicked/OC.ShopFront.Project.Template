// UiLayer/Services/ShippingService.cs
using System.Globalization;
using System.Net.Http.Json;
using OC.LUAC.ApiLayer.DTO.Shipping; // later in the future maybe make a shared dto project so the ui does not depend on the api dto
using OC.LUAC.UiLayer.DTO.Checkout;

namespace OC.LUAC.UiLayer.Services
{
    public class ShippingService
    {
        private readonly HttpClient _http;

        public ShippingService(HttpClient http)
        {
            _http = http;
        }

        // Fetch country list from API
        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            return await _http.GetFromJsonAsync<List<CountryDto>>("shipping-zones/countries")
                   ?? new List<CountryDto>();
        }

        // Call the API shipping quote endpoint
        public async Task<ShippingQuote?> GetQuoteAsync(string country, decimal subtotal, CancellationToken ct = default)
        {
            // Always format decimals using "." instead of ","
            var url = $"shipping-zones/quote?country={country}&subtotal={subtotal.ToString(CultureInfo.InvariantCulture)}";
            return await _http.GetFromJsonAsync<ShippingQuote>(url, ct);
        }
    }
}
