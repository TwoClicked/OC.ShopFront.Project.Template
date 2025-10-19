using OC.LUAC.UiLayer.Components;
using OC.LUAC.UiLayer.Services;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;

using System.Globalization;
using System.Net.Http.Json;
using System.Security.Claims;

// DTOs from UI layer
using OC.LUAC.UiLayer.DTO.Auth;

var builder = WebApplication.CreateBuilder(args);

// ---- UI layer services ----
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ShippingService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<ChatClientService>();
builder.Services.AddScoped<LanguageService>();

// ---- HttpContext + Localization ----
builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization();

var supportedCultures = new[] { "en", "de" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// ---- HttpClient Setup ----
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrEmpty(apiBaseUrl))
{
    throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
}

Console.WriteLine("=========================================");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"ApiBaseUrl: {apiBaseUrl}");
Console.WriteLine("=========================================");

builder.Services.AddTransient<CultureHttpMessageHandler>();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl}/api/");
})
.AddHttpMessageHandler<CultureHttpMessageHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// ✅ Add separate HttpClient for UI endpoints (used by AuthService & AdminAuthService)
builder.Services.AddHttpClient("UiClient", client =>
{
    // Use the same origin as your UI app
    client.BaseAddress = new Uri("https://localhost:7273/");
});

// ---- Authentication & Authorization (Cookie) ----
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "oc.auth";
    options.LoginPath = "/loginRegister";          // unauthenticated → redirect here
    options.AccessDeniedPath = "/loginRegister";   // authenticated but forbidden
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    // If UI is on HTTPS (recommended) Secure is automatic; if running cross-site, consider SameSite=None.
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ---- Razor Components (Interactive Server) ----
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ---- Middleware ----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files (wwwroot)
app.UseStaticFiles();

// Explicit uploads folder
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Localization
app.UseRequestLocalization();

// Antiforgery for interactive server
app.UseAntiforgery();

// *** REQUIRED for [Authorize] to work ***
app.UseAuthentication();
app.UseAuthorization();


// =======================
// UI AUTH ENDPOINTS (cookie minted here)
// =======================

// Customer login: calls API -> issues cookie (Role=Customer)
app.MapPost("/ui/login", async (HttpContext ctx, LoginDto dto, IHttpClientFactory http) =>
{
    var api = http.CreateClient("ApiClient");

    // Call API login to validate and get JWT + profile
    var apiResp = await api.PostAsJsonAsync("customers/login", dto);
    if (!apiResp.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)apiResp.StatusCode);
    }

    var login = await apiResp.Content.ReadFromJsonAsync<LoginResponseDto>();
    if (login is null || string.IsNullOrEmpty(login.Token) || login.Customer is null)
    {
        return Results.Unauthorized();
    }

    // Create cookie claims for UI auth
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, login.Customer.Id.ToString()),
        new(ClaimTypes.Name, login.Customer.Email ?? string.Empty),
        new(ClaimTypes.Email, login.Customer.Email ?? string.Empty),
        new("customerId", login.Customer.Id.ToString()),
        new(ClaimTypes.Role, "Customer")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity));

    // Return the same shape your UI expects (keep JWT if you still use it for API calls)
    return Results.Ok(login);
}).AllowAnonymous();

// Admin login: calls API -> issues cookie (Role=Admin)
app.MapPost("/ui/admin/login", async (HttpContext ctx, LoginDto dto, IHttpClientFactory http) =>
{
    var api = http.CreateClient("ApiClient");

    // Call API admin login
    var apiResp = await api.PostAsJsonAsync("admin/auth/login", dto);
    if (!apiResp.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)apiResp.StatusCode);
    }

    // Expecting an object with token, role = "Admin", admin { Id, Email }
    var payload = await apiResp.Content.ReadFromJsonAsync<AdminLoginResponseDto>();
    if (payload is null || string.IsNullOrEmpty(payload.Token) || payload.Role != "Admin" || payload.Admin is null)
    {
        return Results.Unauthorized();
    }

    // Cookie claims for Admin
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, payload.Admin.Id.ToString()),
        new(ClaimTypes.Name, payload.Admin.Email ?? string.Empty),
        new(ClaimTypes.Email, payload.Admin.Email ?? string.Empty),
        new(ClaimTypes.Role, "Admin")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity));

    return Results.Ok(payload);
}).AllowAnonymous();

// Logout: clears cookie
app.MapPost("/ui/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { status = "LoggedOut" });
});

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
