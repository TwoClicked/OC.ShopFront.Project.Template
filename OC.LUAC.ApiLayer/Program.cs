using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using OC.LUAC.ApiLayer;
using OC.LUAC.ApiLayer.Hubs;
using OC.LUAC.ServiceLayer;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

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

// Swagger
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

// ✅ CORS (allow Accept-Language)
builder.Services.AddCors(o =>
    o.AddPolicy("dev", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Content-Language") // optional: allow client to see culture response
    )
);

// ✅ Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[] { "en", "de" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

// Use Accept-Language as culture provider
localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
{
    new AcceptLanguageHeaderRequestCultureProvider()
};

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Static files
app.UseStaticFiles();

// ✅ Ensure CORS runs BEFORE localization
app.UseCors("dev");

// ✅ Debug raw header BEFORE localization resolves
app.Use(async (context, next) =>
{
    Console.WriteLine("[API] Raw Accept-Language header: " +
        context.Request.Headers["Accept-Language"].ToString());
    await next();
});

// ✅ Apply localization
app.UseRequestLocalization(localizationOptions);

// ✅ Debug resolved culture
app.Use(async (context, next) =>
{
    var feature = context.Features.Get<IRequestCultureFeature>();
    Console.WriteLine($"[API] Resolved Culture: {feature?.RequestCulture.Culture}");
    await next();
});

// Auth pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();
