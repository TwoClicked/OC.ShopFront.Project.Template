using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OC.LUAC.ServiceLayer.Interfaces;
using OC.LUAC.ObjectLayer.Accounts;

namespace OC.LUAC.ApiLayer.Startup
{
    public class AdminSeedHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<AdminSeedHostedService> _logger;

        public AdminSeedHostedService(IServiceProvider services, IConfiguration config, ILogger<AdminSeedHostedService> logger)
        {
            _services = services;
            _config = config;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var admins = scope.ServiceProvider.GetRequiredService<IAdminUserService>();

            // If there is already at least one admin, nothing to do
            var any = (await admins.GetAllAsync(includeInactive: true)).Any();
            if (any) return;

            var email = _config["AdminAuth:Email"];
            var password = _config["AdminAuth:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Admin seeding skipped: AdminAuth:Email/Password not configured.");
                return;
            }

            var admin = new AdminUser
            {
                Email = email.Trim(),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await admins.CreateAsync(admin, password);
            _logger.LogInformation("Seeded default admin account: {Email}", email);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
