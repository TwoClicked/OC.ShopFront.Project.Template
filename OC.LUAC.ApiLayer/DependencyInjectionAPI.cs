// OC.LUAC.ApiLayer/DependencyInjectionAPI.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OC.LUAC.ApiLayer.Auth;
using OC.LUAC.ApiLayer.Storage;

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
                });

            services.AddAuthorization();

            return services;
        }
    }
}
