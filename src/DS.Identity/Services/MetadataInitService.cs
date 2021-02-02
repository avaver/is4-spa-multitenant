using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DS.Identity.Services
{
    public class MetadataInitService: IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public MetadataInitService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var metadataService = scope.ServiceProvider.GetRequiredService<KeyMetadataService>();
            await metadataService.Init();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}