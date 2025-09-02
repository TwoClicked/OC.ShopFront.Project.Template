// OC.LUAC.ApiLayer/DependencyInjectionAPI.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OC.LUAC.ApiLayer.Auth;
using OC.LUAC.ApiLayer.Storage;
using OC.LUAC.ApiLayer.Startup;

namespace OC.LUAC.ApiLayer
{
    public static class DependencyInjectionAPI
    {
        public static IServiceCollection AddApiLayerServices(this IServiceCollection services, IConfiguration config)
        {
            // File storage for product images
            services.AddScoped<IImageStorage, LocalImageStorage>();

            // Bind JWT options from appsettings.json
            services.Configure<JwtOptions>(config.GetSection("Jwt"));
            var jwt = config.GetSection("Jwt").Get<JwtOptions>()
                      ?? throw new InvalidOperationException("Missing 'Jwt' section in appsettings.json.");

            // Token service
            services.AddSingleton<ITokenService, JwtTokenService>();

            // Authentication + Authorization
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ClockSkew = TimeSpan.Zero
                    };

                    // 🔍 Debug logging for JWT events
                    o.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            Console.WriteLine($"❌ JWT validation failed: {ctx.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = ctx =>
                        {
                            var claims = string.Join(", ", ctx.Principal.Claims.Select(c => $"{c.Type}={c.Value}"));
                            Console.WriteLine($"✅ JWT validated for: {ctx.Principal.Identity?.Name}");
                            Console.WriteLine($"Claims: {claims}");
                            return Task.CompletedTask;
                        },
                        OnChallenge = ctx =>
                        {
                            Console.WriteLine($"⚠️ JWT challenge: {ctx.Error}, {ctx.ErrorDescription}");
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            // Register the seeding hosted service (runs once on startup)
            services.AddHostedService<AdminSeedHostedService>();

            return services;
        }
    }
}
