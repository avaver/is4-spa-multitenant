using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DS.Identity.AppIdentity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DS.Identity.Services.Hosted
{
    public class DataSeedingService: IHostedService
    {
        private const string DefaultPassword = "1111";
        
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataSeedingService> _logger;
        
        public DataSeedingService(IServiceProvider serviceProvider, ILogger<DataSeedingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Seeding test data...");
            await CreateTestUsers();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        
        private async Task CreateTestUsers()
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<AppUserManager>();
            foreach (var user in Users)
            {
                var dbUser = await userManager.FindByIdAsync(user.Id);
                if (dbUser != null)
                {
                    _logger.LogInformation("User {0} @ {1} already exists, deleting it", user.UserName, user.TenantName);
                    await userManager.DeleteAsync(dbUser);
                }

                if (!(await userManager.CreateAsync(user, DefaultPassword)).Succeeded)
                {
                    _logger.LogError("Failed to create user {0} @ {1}", user.UserName, user.TenantName);
                }
                else
                {
                    _logger.LogInformation("User {0} @ {1} created", user.UserName, user.TenantName);
                }
            }
        }

        private static IEnumerable<AppUser> Users =>
            new[]
            {
                new AppUser
                {
                    Id = "5B3654F2-28E2-487B-BC6E-939AD2FFB842",
                    UserName = "sd",
                    TenantName = "superdent",
                    IsClinicAdmin = false
                },
                new AppUser
                {
                    Id = "1071C712-70A1-47A4-A4BE-59B1A9F0D188",
                    UserName = "ht",
                    TenantName = "happyteeth",
                    IsClinicAdmin = false
                },
                new AppUser
                {
                    Id = "772D9024-A03B-4607-8126-393AEFD88351",
                    UserName = "adm",
                    TenantName = "superdent",
                    IsClinicAdmin = true
                },
                new AppUser
                {
                    Id = "48295BA6-5E3C-469C-9E36-5FA4EDD7AE0F",
                    UserName = "adm",
                    TenantName = "happyteeth",
                    IsClinicAdmin = true
                }
            };
    }
}