using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using OC.LUAC.UiLayer.DTO.Auth;
using OC.LUAC.UiLayer.DTO.Order;
using OC.LUAC.UiLayer.DTO.AdminDash;



namespace OC.LUAC.UiLayer.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "authToken";
        private const string ProfileKey = "userProfile";

        public event Action? OnAuthStateChanged;

        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        // ====================
        // REGISTER
        // ====================
        public async Task<(bool Success, string? Status, string? Message)> RegisterAsync(RegisterDto dto)
        {
            var response = await _http.PostAsJsonAsync("customers/register", dto);

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

            return (true, null, null); // fallback
        }


        // ====================
        // LOGIN
        // ====================
        public async Task<(bool Success, bool Disabled)> LoginAsync(LoginDto dto)
        {
            var response = await _http.PostAsJsonAsync("customers/login", dto);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Account exists but disabled
                return (false, true);
            }

            if (!response.IsSuccessStatusCode)
                return (false, false); // Invalid credentials

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (loginResponse is null || string.IsNullOrEmpty(loginResponse.Token))
                return (false, false);

            await _localStorage.SetItemAsync(TokenKey, loginResponse.Token);
            await _localStorage.SetItemAsync(ProfileKey, loginResponse.Customer);

            OnAuthStateChanged?.Invoke();
            return (true, false);
        }


        // ====================
        // LOGOUT
        // ====================
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(ProfileKey);
            OnAuthStateChanged?.Invoke();
        }

        // ====================
        // CHECK LOGIN
        // ====================
        public async Task<bool> IsLoggedInAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // check exp claim
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await LogoutAsync(); // auto logout
                return false;
            }

            return true;
        }

        public async Task<string?> GetTokenAsync() =>
            await _localStorage.GetItemAsync<string>(TokenKey);

        // ====================
        // PROFILE (cached + refresh)
        // ====================
        public async Task<CustomerProfileDto?> GetProfileAsync(bool refresh = false)
        {
            // load from cache
            var profile = await _localStorage.GetItemAsync<CustomerProfileDto>(ProfileKey);
            if (!refresh && profile != null) return profile;

            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return profile;

            using var request = new HttpRequestMessage(HttpMethod.Get, "customers/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode) return profile;

                var latest = await response.Content.ReadFromJsonAsync<CustomerProfileDto>();
                if (latest != null)
                {
                    await _localStorage.SetItemAsync(ProfileKey, latest);
                    return latest;
                }
            }
            catch
            {
                // ignore errors and fall back to cached
            }

            return profile;
        }

        // ====================
        // UPDATE PROFILE
        // ====================
        public async Task<bool> UpdateProfileAsync(UpdateCustomerDto dto)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("⚠️ No token found in local storage.");
                return false;
            }

            var url = "customers/me";
            using var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(dto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Debug logs
            Console.WriteLine("---- UpdateProfileAsync Request ----");
            Console.WriteLine($"URL: {_http.BaseAddress}{url}");
            Console.WriteLine($"Authorization: Bearer {token.Substring(0, 20)}..."); // only log start of token
            Console.WriteLine($"Payload: {System.Text.Json.JsonSerializer.Serialize(dto)}");
            Console.WriteLine("-----------------------------------");

            var response = await _http.SendAsync(request);

            // Response log
            Console.WriteLine($"Response: {(int)response.StatusCode} {response.ReasonPhrase}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Body: {body}");
                return false;
            }

            // Refresh profile cache
            var refreshed = await GetProfileAsync(refresh: true);
            Console.WriteLine($"Profile refresh success: {refreshed != null}");
            return refreshed != null;
        }


        // ====================
        // CHANGE PASSWORD
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

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // ====================
        // RESET PASSWORD
        // ====================
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var response = await _http.PostAsJsonAsync("customers/forgot-password", new { Email = email });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var response = await _http.PostAsJsonAsync("customers/reset-password", dto);
            return response.IsSuccessStatusCode;
        }

        // ====================
        // DELETE ACCOUNT
        // ====================
        public async Task<bool> DeleteAccountAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            using var request = new HttpRequestMessage(HttpMethod.Delete, "customers/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }


        // ====================
        // GET ORDERS
        // ====================
        public async Task<List<AdminOrderSummaryDto>?> GetOrdersAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            using var request = new HttpRequestMessage(HttpMethod.Get, "orders/my");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var apiOrders = await response.Content.ReadFromJsonAsync<List<OrderSummaryDto>>();
                if (apiOrders == null) return null;

                // Map API DTO → UI DTO
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
        // CANCEL ORDER
        // ====================
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            using var request = new HttpRequestMessage(HttpMethod.Put, $"orders/{orderId}/cancel-me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
