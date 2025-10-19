using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using OC.LUAC.UiLayer.DTO.Auth;
using OC.LUAC.UiLayer.DTO.Order;
using OC.LUAC.UiLayer.DTO.AdminDash;

namespace OC.LUAC.UiLayer.Services
{
    public class AuthService
    {
        private readonly HttpClient _apiHttp;  // for API calls (customers/me, orders, etc.)
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _js;

        private const string TokenKey = "authToken";
        private const string ProfileKey = "userProfile";

        public event Action? OnAuthStateChanged;

        public AuthService(IHttpClientFactory factory, ILocalStorageService localStorage, IJSRuntime js)
        {
            _apiHttp = factory.CreateClient("ApiClient");
            _localStorage = localStorage;
            _js = js;
        }

        // ====================
        // REGISTER (API)
        // ====================
        public async Task<(bool Success, string? Status, string? Message)> RegisterAsync(RegisterDto dto)
        {
            var response = await _apiHttp.PostAsJsonAsync("customers/register", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (result != null && result.TryGetValue("status", out var status))
            {
                string? msg = result.ContainsKey("message") ? result["message"]?.ToString() : null;
                return (true, status?.ToString(), msg);
            }

            return (true, null, null);
        }

        // ====================
        // LOGIN via JS fetch -> /ui/login (sets cookie in browser)
        // ====================
        public async Task<(bool Success, bool Disabled)> LoginAsync(LoginDto dto)
        {
            // Calls window.uiLogin(dto) from wwwroot/app.js
            LoginResponseDto? loginResponse;
            try
            {
                loginResponse = await _js.InvokeAsync<LoginResponseDto>("uiLogin", dto);
            }
            catch
            {
                return (false, false);
            }

            // 403 is handled inside uiLogin by throwing; here we only get JSON or nothing
            if (loginResponse is null || loginResponse.Customer is null)
                return (false, false);

            // keep JWT for API bearer calls
            await _localStorage.SetItemAsync(TokenKey, loginResponse.Token);
            await _localStorage.SetItemAsync(ProfileKey, loginResponse.Customer);

            OnAuthStateChanged?.Invoke();
            return (true, false);
        }

        // ====================
        // LOGOUT via JS fetch -> /ui/logout (clears cookie)
        // ====================
        public async Task LogoutAsync()
        {
            try { await _js.InvokeVoidAsync("uiLogout"); } catch { /* ignore */ }
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(ProfileKey);
            OnAuthStateChanged?.Invoke();
        }

        // ====================
        // CHECK LOGIN (token freshness)
        // ====================
        public async Task<bool> IsLoggedInAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await LogoutAsync();
                return false;
            }

            return true;
        }

        public async Task<string?> GetTokenAsync() =>
            await _localStorage.GetItemAsync<string>(TokenKey);

        // ====================
        // PROFILE (API; cached + refresh)
        // ====================
        public async Task<CustomerProfileDto?> GetProfileAsync(bool refresh = false)
        {
            var profile = await _localStorage.GetItemAsync<CustomerProfileDto>(ProfileKey);
            if (!refresh && profile != null) return profile;

            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return profile;

            using var request = new HttpRequestMessage(HttpMethod.Get, "customers/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _apiHttp.SendAsync(request);
                if (!response.IsSuccessStatusCode) return profile;

                var latest = await response.Content.ReadFromJsonAsync<CustomerProfileDto>();
                if (latest != null)
                {
                    await _localStorage.SetItemAsync(ProfileKey, latest);
                    return latest;
                }
            }
            catch { /* ignore */ }

            return profile;
        }

        // ====================
        // UPDATE PROFILE (API)
        // ====================
        public async Task<bool> UpdateProfileAsync(UpdateCustomerDto dto)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            using var request = new HttpRequestMessage(HttpMethod.Put, "customers/me")
            {
                Content = JsonContent.Create(dto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _apiHttp.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var refreshed = await GetProfileAsync(refresh: true);
            return refreshed != null;
        }

        // ====================
        // CHANGE PASSWORD (API)
        // ====================
        public async Task<bool> ChangePasswordAsync(ChangePasswordFormDto dto)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            using var request = new HttpRequestMessage(HttpMethod.Put, "customers/me/password")
            {
                Content = JsonContent.Create(dto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _apiHttp.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // ====================
        // RESET PASSWORD (API)
        // ====================
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var response = await _apiHttp.PostAsJsonAsync("customers/forgot-password", new { Email = email });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var response = await _apiHttp.PostAsJsonAsync("customers/reset-password", dto);
            return response.IsSuccessStatusCode;
        }

        // ====================
        // DELETE ACCOUNT (API)
        // ====================
        public async Task<bool> DeleteAccountAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            using var request = new HttpRequestMessage(HttpMethod.Delete, "customers/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _apiHttp.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // ====================
        // GET ORDERS (API)
        // ====================
        public async Task<List<AdminOrderSummaryDto>?> GetOrdersAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            using var request = new HttpRequestMessage(HttpMethod.Get, "orders/my");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _apiHttp.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var apiOrders = await response.Content.ReadFromJsonAsync<List<OrderSummaryDto>>();
                if (apiOrders == null) return null;

                return apiOrders.Select(o => new AdminOrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    Discount = o.Discount,
                    ShippingCost = o.ShippingCost,
                    TotalAfterDiscount = o.TotalAfterDiscount,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    ShippingAddress = o.ShippingAddress,
                    ShippingCity = o.ShippingCity,
                    ShippingPostalCode = o.ShippingPostalCode,
                    ShippingCountry = o.ShippingCountry,
                    TrackingNumber = o.TrackingNumber,
                    TrackingUrl = o.TrackingUrl,
                    Items = o.Items.Select(i => new AdminOrderItemDto
                    {
                        ProductName = i.ProductName,
                        Size = i.Size,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        // ====================
        // CANCEL ORDER (API)
        // ====================
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            using var request = new HttpRequestMessage(HttpMethod.Put, $"orders/{orderId}/cancel-me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _apiHttp.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
