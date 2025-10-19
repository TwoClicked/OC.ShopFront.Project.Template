using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using OC.LUAC.UiLayer.DTO.Auth;

namespace OC.LUAC.UiLayer.Services
{
    public class AdminAuthService
    {
        private readonly HttpClient _apiHttp;      // API calls (if/when needed)
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _js;

        private const string TokenKey = "adminToken";

        public event Action? OnAuthStateChanged;

        public AdminAuthService(IHttpClientFactory factory, ILocalStorageService localStorage, IJSRuntime js)
        {
            _apiHttp = factory.CreateClient("ApiClient");
            _localStorage = localStorage;
            _js = js;
        }

        // ====================
        // LOGIN via JS fetch -> /ui/admin/login (sets cookie in browser)
        // ====================
        public async Task<bool> LoginAsync(LoginDto dto)
        {
            // Calls window.uiAdminLogin(dto) from wwwroot/app.js
            AdminLoginResponseDto? result;
            try
            {
                result = await _js.InvokeAsync<AdminLoginResponseDto>("uiAdminLogin", dto);
            }
            catch
            {
                return false;
            }

            if (result == null || string.IsNullOrEmpty(result.Token) || !string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return false;

            await SafeSetItemAsync(TokenKey, result.Token);
            OnAuthStateChanged?.Invoke();
            return true;
        }

        // ====================
        // LOGOUT via JS fetch -> /ui/logout (clears cookie)
        // ====================
        public async Task LogoutAsync()
        {
            try { await _js.InvokeVoidAsync("uiLogout"); } catch { /* ignore */ }
            await SafeRemoveItemAsync(TokenKey);
            OnAuthStateChanged?.Invoke();
        }

        // ====================
        // CHECK LOGIN (local token presence only; cookie auth gate is server-side)
        // ====================
        public async Task<bool> IsLoggedInAsync()
        {
            var token = await SafeGetItemAsync<string>(TokenKey);
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string?> GetTokenAsync() =>
            await SafeGetItemAsync<string>(TokenKey);

        // -------- Safe local storage helpers --------
        private async Task<T?> SafeGetItemAsync<T>(string key)
        {
            try { return await _localStorage.GetItemAsync<T>(key); }
            catch { return default; }
        }

        private async Task SafeSetItemAsync<T>(string key, T value)
        {
            try { await _localStorage.SetItemAsync(key, value); } catch { }
        }

        private async Task SafeRemoveItemAsync(string key)
        {
            try { await _localStorage.RemoveItemAsync(key); } catch { }
        }
    }
}
