using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Timeout;

namespace HoneyComb.Server.CommandHandlers
{
    public static class HttpClientFactoryExtensions
    {
        public static IServiceCollection AddHoneyHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient(Constants.HttpClientNames.Default)
                .AddDefaultPollyPolicy();

            return services;
        }

        public static IPolicyRegistry<string> AddDefaultPollyPolicy(this IHttpClientBuilder builder,
            int retryCount = 3,
            int sleepDurationMilliseconds = 100,
            int timeoutSeconds = 8)
        {
            Polly.Retry.AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .WaitAndRetryAsync(retryCount: retryCount, sleepDurationProvider: (retryCount) => TimeSpan.FromMilliseconds(sleepDurationMilliseconds));

            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(seconds: timeoutSeconds, TimeoutStrategy.Optimistic);

            IPolicyRegistry<string>? registry = builder.Services.AddPolicyRegistry();
            registry.Add(Constants.HttpClientNames.Default, Policy.WrapAsync(retryPolicy, timeoutPolicy));
            builder.AddPolicyHandlerFromRegistry(Constants.HttpClientNames.Default);

            return registry;
        }
    }
}
