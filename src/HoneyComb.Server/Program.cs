

using System;
using Fabron;
using Fabron.Events;
using Fabron.Providers.PostgreSQL;
using HoneyComb.Server;
using HoneyComb.Server.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


await Host.CreateDefaultBuilder()
    .ConfigureLogging(logging =>
    {
        logging.Configure(options =>
        {
            options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
                | ActivityTrackingOptions.TraceId;
        });
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<MetricsHostedService>();
    })
    .UseFabron((context, silo) =>
    {
        silo
            .AddActivityPropagation()
            .AddPrometheusTelemetryConsumer()
            .Configure<StatisticsOptions>(options =>
            {
                options.LogWriteInterval = TimeSpan.FromMilliseconds(-1);
            })
            .Configure<CronJobOptions>(options =>
            {
                options.CronFormat = Cronos.CronFormat.IncludeSeconds;
            })
            .ConfigureLogging(logging =>
            {
                logging.Configure(options =>
                {
                    options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
                        | ActivityTrackingOptions.TraceId;
                });
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHoneyHttpClient();
            });
        if (context.HostingEnvironment.IsEnvironment("Localhost"))
        {
            silo
                .UseLocalhostClustering()
                .UseInMemory();
        }
        else
        {
            silo.UseKubernetesHosting()
                .UsePosgreSQL(context.Configuration.GetSection("PostgreSQL"))
                .SetEventListener<NoopJobEventListener, NoopCronJobEventListener>()
                .ConfigureServices(services =>
                {
                    services.AddOpenTelemetryTracing(builder
                        => builder
                            .AddSource("orleans.runtime.graincall")
                            .AddHttpClientInstrumentation()
                            .AddOtlpExporter()
                    );
                });
        }
    })
    .RunConsoleAsync();
