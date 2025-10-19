using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OC.LUAC.ApiLayer;
using OC.LUAC.ApiLayer.Hubs;
using OC.LUAC.ApiLayer.Startup;
using OC.LUAC.DataLayer;
using OC.LUAC.ServiceLayer;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// API layer DI
builder.Services.AddApiLayerServices(builder.Configuration);
builder.Services.AddProjectServices(builder.Configuration);

// QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// Controllers + JSON options
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// SignalR
builder.Services.AddSignalR();

// Swagger (enabled everywhere 🚀)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LUAC API", Version = "v1" });
    c.CustomSchemaIds(t => t.FullName);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ CORS (restrict to UI domain in production)
builder.Services.AddCors(o =>
    o.AddPolicy("prod", p => p
        .WithOrigins("https://luac-ui-win.azurewebsites.net") // UI App Service domain
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Content-Language")
    )
);

// ✅ Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var supportedCultures = new[] { "en", "de" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
{
    new AcceptLanguageHeaderRequestCultureProvider()
};

var app = builder.Build();

// ✅ Log DB info on startup (no passwords)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var conn = db.Database.GetDbConnection();
    Console.WriteLine($"[Startup] Connected to DB: {conn.Database} on {conn.DataSource}");
}

// ✅ Swagger always on (consider securing in prod)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LUAC API v1");
    c.RoutePrefix = "swagger";
});

// ✅ Global error logging middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Request exception: " + ex.ToString());
        throw;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("prod");

// Debug Accept-Language header
app.Use(async (context, next) =>
{
    Console.WriteLine("[API] Raw Accept-Language header: " +
        context.Request.Headers["Accept-Language"].ToString());
    await next();
});

// Apply localization
app.UseRequestLocalization(localizationOptions);

// Debug resolved culture
app.Use(async (context, next) =>
{
    var feature = context.Features.Get<IRequestCultureFeature>();
    Console.WriteLine($"[API] Resolved Culture: {feature?.RequestCulture.Culture}");
    await next();
});

// Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");


// Seed default admin user
// await app.Services.SeedDefaultAdminAsync(app.Configuration); Only needed on a fresh launch, but if there is an admin not needed


// ✅ Catch startup SQL errors
try
{
    app.Run();
}
catch (SqlException ex)
{
    Console.WriteLine($"❌ SQL Error {ex.Number}: {ex.Message}");
    foreach (SqlError err in ex.Errors)
    {
        Console.WriteLine($"  -> {err.Message} (Line {err.LineNumber}, State {err.State}, Class {err.Class})");
    }
    throw;
}
catch (Exception ex)
{
    Console.WriteLine("❌ Unhandled exception at startup: " + ex);
    throw;
}
