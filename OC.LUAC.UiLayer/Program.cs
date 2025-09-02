using OC.LUAC.UiLayer.Components;
using OC.LUAC.UiLayer.Services;
using Blazored.LocalStorage;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//Ui layer service injections (Might make a UI Di class for this later)

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ShippingService>();
builder.Services.AddScoped<AuthService>();



// ---- HttpClient Setup ----
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrEmpty(apiBaseUrl))
{
    throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
}

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl}/api/");
});

// Default HttpClient resolves to ApiClient
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

// Make System.Text.Json case-insensitive globally
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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
