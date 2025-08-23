using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OC.LUAC.ApiLayer;
using OC.LUAC.ServiceLayer;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// API layer DI (JWT, token service, image storage, etc.)
builder.Services.AddApiLayerServices(builder.Configuration);

// Service layer DI
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

// Swagger (+ Bearer support)
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

// Dev CORS (tighten for prod)
builder.Services.AddCors(o =>
    o.AddPolicy("dev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("dev");

// Auth pipeline
app.UseAuthentication();   // must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
