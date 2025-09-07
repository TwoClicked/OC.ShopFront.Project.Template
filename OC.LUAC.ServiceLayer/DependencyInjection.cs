
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
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

            // Services registration

            services.AddScoped<IProductService, ProductService>(); 
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductVariantService, ProductVariantService>();
            services.AddScoped<IStockActionService, StockActionService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IShippingZoneService, ShippingZoneService>();

            return services;

        }
    }
}
