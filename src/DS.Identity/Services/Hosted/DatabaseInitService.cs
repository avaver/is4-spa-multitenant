using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DS.Identity.Extensions;
using DS.Identity.IdentityServer;
using DS.Identity.AppIdentity;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DS.Identity.Services.Hosted
{
    public class DatabaseInitService: IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitService> _logger;

        public DatabaseInitService(IServiceProvider serviceProvider, ILogger<DatabaseInitService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Applying database migrations...");
            using var scope = _serviceProvider.CreateScope();
            var idb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
            var cdb = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            var odb = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            await idb.Database.MigrateAsync(cancellationToken);
            await cdb.Database.MigrateAsync(cancellationToken);
            await odb.Database.MigrateAsync(cancellationToken);
            
            _logger.LogInformation("Seeding IdentityServer configuration...");
            cdb.IdentityResources.Recreate(Config.IdentityResources.Select(o => o.ToEntity()), (de, le) => de.Name == le.Name);
            cdb.ApiScopes.Recreate(Config.ApiScopes.Select(o => o.ToEntity()), (de, le) => de.Name == le.Name);
            cdb.Clients.Recreate(Config.Clients.Select(o => o.ToEntity()), (de, le) => de.ClientId == le.ClientId);
            await cdb.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}