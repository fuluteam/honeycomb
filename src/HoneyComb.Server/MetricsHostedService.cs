

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace HoneyComb.Server
{
    public class MetricsHostedService : IHostedService
    {
        private readonly KestrelMetricServer _metricServer;
        private readonly ILogger<MetricsHostedService> _logger;

        public MetricsHostedService(ILogger<MetricsHostedService> logger)
        {
            _metricServer = new KestrelMetricServer(port: 80);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _metricServer.Start();
            _logger.LogInformation("Metrics server started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => _metricServer.StopAsync().WaitAsync(cancellationToken);
    }
}
