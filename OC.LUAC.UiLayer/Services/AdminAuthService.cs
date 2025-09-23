using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using OC.LUAC.UiLayer.DTO.Auth;

namespace OC.LUAC.UiLayer.Services
{
    public class AdminAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "adminToken";

        public event Action? OnAuthStateChanged;

        public AdminAuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        // ====================
        // LOGIN
        // ====================
        public async Task<bool> LoginAsync(LoginDto dto)
        {
            var response = await _http.PostAsJsonAsync("admin/auth/login", dto);
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<AdminLoginResponseDto>();
            if (result == null || string.IsNullOrEmpty(result.Token)) return false;

            // Decode role from token
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);
            var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (role != "Admin" && result.Role != "Admin") return false;

            await SafeSetItemAsync(TokenKey, result.Token);

            OnAuthStateChanged?.Invoke();
            return true;
        }

        // ====================
        // LOGOUT
        // ====================
        public async Task LogoutAsync()
        {
            await SafeRemoveItemAsync(TokenKey);
            OnAuthStateChanged?.Invoke();
        }

        // ====================
        // CHECK LOGIN
        // ====================
        public async Task<bool> IsLoggedInAsync()
        {
            var token = await SafeGetItemAsync<string>(TokenKey);
            Console.WriteLine("🔑 AdminAuthService.IsLoggedInAsync called");
            Console.WriteLine($"Token found? {(string.IsNullOrEmpty(token) ? "No" : "Yes")}");

            if (string.IsNullOrEmpty(token)) return false;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Console.WriteLine($"Role claim: {string.Join(",", jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value))}");

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                Console.WriteLine("❌ Token expired, logging out admin");
                await LogoutAsync();
                return false;
            }

            var isAdmin = jwt.Claims.Any(c => c.Type == "role" && c.Value == "Admin");
            Console.WriteLine($"✅ IsAdmin? {isAdmin}");
            return isAdmin;
        }

        public async Task<string?> GetTokenAsync() =>
            await SafeGetItemAsync<string>(TokenKey);

        // ====================
        // SAFE LOCAL STORAGE ACCESS
        // ====================
        private async Task<T?> SafeGetItemAsync<T>(string key)
        {
            try
            {
                return await _localStorage.GetItemAsync<T>(key);
            }
            catch (InvalidOperationException)
            {
                // JS not ready (likely prerendering)
                return default;
            }
        }

        private async Task SafeSetItemAsync<T>(string key, T value)
        {
            try
            {
                await _localStorage.SetItemAsync(key, value);
            }
            catch (InvalidOperationException)
            {
                // Ignore if JS not available
            }
        }

        private async Task SafeRemoveItemAsync(string key)
        {
            try
            {
                await _localStorage.RemoveItemAsync(key);
            }
            catch (InvalidOperationException)
            {
                // Ignore if JS not available
            }
        }
    }
}
