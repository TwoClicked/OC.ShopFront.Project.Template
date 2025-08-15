using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;
using System.Net.NetworkInformation;

namespace OC.LUAC.ApiLayer.Startup
{
    public static class AdminSeeder
    {

        public static async Task SeedDefaultAdminAsync(this IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            var admins = scope.ServiceProvider.GetRequiredService<IAdminUserService>();

            // if there is an admin, we do nothing
            var existing = await admins.GetAllAsync();
            if (existing != null) return;

            var email = config["AdminAuth:Email"];
            var password = config["AdminAuth:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return; // Nothing to seed
            var admin = new AdminUser
            {
                Email = email.Trim(),
                Role = "Admin",        
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await admins.CreateAsync(admin, password);

        }

    }
}
