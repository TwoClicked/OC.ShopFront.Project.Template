using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OC.LUAC.ApiLayer;
using OC.LUAC.ServiceLayer;

var builder = WebApplication.CreateBuilder(args);

//Api layer DI
builder.Services.AddApiLayerServices();

// Service layer DI
builder.Services.AddProjectServices(builder.Configuration);



// Controllers + JSON options (match Syntra behavior)
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Luac API", Version = "v1" });
    // (We’ll add JWT security to Swagger once auth is wired)
});

// Dev CORS for now (tighten later)
builder.Services.AddCors(o =>
    o.AddPolicy("dev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("dev");
app.UseStaticFiles(); // Enables serving wwwroot content
app.UseAuthorization();

app.MapControllers();
app.Run();
