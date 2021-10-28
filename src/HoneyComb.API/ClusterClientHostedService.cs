


using System.Threading;
using System.Threading.Tasks;
using Fabron.Providers.PostgreSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;

namespace HoneyComb.API
{
    public class ClusterClientHostedService : IHostedService
    {
        private readonly ILogger<ClusterClientHostedService> _logger;

        public ClusterClientHostedService(ILogger<ClusterClientHostedService> logger,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            IClientBuilder builder = new ClientBuilder()
                .AddActivityPropagation()
                .Configure<ClusterOptions>(configuration.GetSection("Cluster"));
            if (environment.IsEnvironment("Localhost"))
            {
                builder.UseLocalhostClustering();
            }
            else
            {
                builder.UsePostgreSQLClustering(configuration["PostgreSQL:ConnectionString"]);
            }
            Client = builder.Build();
            _logger = logger;
            Environment = environment;
            Configuration = configuration;
        }

        public IClusterClient Client { get; }
        public IHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Client.Connect();
            _logger.LogInformation("Conencted to silos");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.Close();
            _logger.LogInformation("Connection closed");
        }
    }
}
