using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.JSInterop;
using System.Globalization;

namespace OC.LUAC.UiLayer.Services
{
    public class LanguageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LanguageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Save culture to both cookie and localStorage (so server + client agree).
        /// </summary>
        public async Task SetLanguageAsync(string culture)
        {
            // Cookie (for server middleware)
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            var cookieName = CookieRequestCultureProvider.DefaultCookieName;

            var script = $@"
                document.cookie = '{cookieName}={Uri.EscapeDataString(cookieValue)}; path=/; max-age=31536000; SameSite=Lax';
                localStorage.setItem('culture', '{culture}');
                console.log('[JS] Set culture cookie + localStorage: {culture}');
            ";

            await _jsRuntime.InvokeVoidAsync("eval", script);
            Console.WriteLine($"[LanguageService] Set culture cookie + localStorage: {culture}");
        }

        /// <summary>
        /// Get culture from localStorage (preferred for UI reload).
        /// </summary>
        public async Task<string?> GetLanguageFromLocalStorageAsync()
        {
            var culture = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "culture");
            return string.IsNullOrWhiteSpace(culture) ? null : culture;
        }

        /// <summary>
        /// Get culture from cookie (fallback).
        /// </summary>
        public async Task<string?> GetLanguageFromCookieAsync()
        {
            var cookieName = CookieRequestCultureProvider.DefaultCookieName;
            var script = $"(document.cookie.split('; ').find(r => r.startsWith('{cookieName}=')) || '').split('=')[1]";
            var value = await _jsRuntime.InvokeAsync<string>("eval", script);

            if (string.IsNullOrEmpty(value))
                return null;

            var result = CookieRequestCultureProvider.ParseCookieValue(Uri.UnescapeDataString(value));
            return result?.UICultures.FirstOrDefault().Value;
        }

        /// <summary>
        /// Fallback to thread culture if nothing is found.
        /// </summary>
        public string GetCurrentLanguage()
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }
    }
}
