using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DS.Identity.Services.Hosted
{
    public class MetadataInitService: IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MetadataInitService> _logger;

        public MetadataInitService(IServiceProvider serviceProvider, ILogger<MetadataInitService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing key metadata service...");
            using var scope = _serviceProvider.CreateScope();
            var metadataService = scope.ServiceProvider.GetRequiredService<FidoMetadataService>();
            await metadataService.Init();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}