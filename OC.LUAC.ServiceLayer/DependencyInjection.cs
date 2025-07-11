
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using DiscordRPC;
using Microsoft.Extensions.Configuration;
using OC.LUAC.ServiceLayer.Services;
using OC.LUAC.ServiceLayer.Interfaces;


namespace OC.LUAC.ServiceLayer
{
    /// <summary>
    /// Dependency injection configuration for the service layer.
    /// </summary>
    public static class DependencyInjection
    {

        public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Add DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // TODO: add services later 
            
            services.AddScoped<IProductService, ProductService>(); 
            services.AddScoped<ICategoryService, CategoryService>();

            return services;

        }
    }
}
