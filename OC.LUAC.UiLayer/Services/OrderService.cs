// UiLayer/Services/OrderService.cs
using System.Net.Http.Json;
using OC.LUAC.UiLayer.DTO.Checkout;
using OC.LUAC.ApiLayer.DTO.Order;
using OC.LUAC.ApiLayer.DTO.Product; // Shared contracts for API communication

namespace OC.LUAC.UiLayer.Services
{
    public class OrderService
    {
        private readonly HttpClient _http;

        public OrderService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiLayer.DTO.Order.OrderResponseDto> PlaceOrderAsync(CheckoutRequestDto payload, CancellationToken ct = default)
        {
            // ✅ Map UI DTO -> API DTO
            var dto = new CreateOrderDto
            {
                Email = payload.Shipping.Email,
                FirstName = payload.Shipping.FirstName,
                LastName = payload.Shipping.LastName,
                Language = "en",

                ShippingStreet = payload.Shipping.Line1,
                ShippingNumber = payload.Shipping.Line2 ?? "",
                ShippingPostalCode = payload.Shipping.PostalCode,
                ShippingCity = payload.Shipping.City,
                ShippingCountry = payload.Shipping.Country,

                Items = payload.Items.Select(i => new CreateOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price,
                    ProductName = "", // API snapshots from DB
                    Size = i.Size
                }).ToList()
            };

            var resp = await _http.PostAsJsonAsync("orders", dto, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Error while placing order ({resp.StatusCode}): {error}");
            }

            var result = await resp.Content.ReadFromJsonAsync<ApiLayer.DTO.Order.OrderResponseDto>(cancellationToken: ct);

            if (result == null)
                throw new InvalidOperationException("API did not return an order response.");

            return result;
        }
    }
}
