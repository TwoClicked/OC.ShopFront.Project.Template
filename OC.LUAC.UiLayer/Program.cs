using OC.LUAC.UiLayer.Components;
using OC.LUAC.UiLayer.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

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

// ✅ Localization – no ResourcesPath override, so .resx can live next to Razor components
builder.Services.AddLocalization();

var supportedCultures = new[] { "en", "de" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();

    // ✅ Cookie first, then Accept-Language
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

// ---- Razor Components ----
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

// ✅ Localization middleware
app.UseRequestLocalization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
