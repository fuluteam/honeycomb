

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fabron.Providers.PostgreSQL;
using HoneyComb.API;
using HoneyComb.API.Resources.Apps;
using HoneyComb.API.Services;
using HoneyComb.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Orleans;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
        | ActivityTrackingOptions.TraceId;
});

builder.Services
    .AddHttpClient()
    .AddOpenTelemetryTracing(builder
        => builder
            .AddSource("orleans.runtime.graincall")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter()
    )
    .AddSingleton<ClusterClientHostedService>()
    .AddSingleton<IHostedService>(_ => _.GetRequiredService<ClusterClientHostedService>())
    .AddSingleton(_ => _.GetRequiredService<ClusterClientHostedService>().Client)
    .AddSingleton<IGrainFactory>(_ => _.GetRequiredService<IClusterClient>())
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v2", new OpenApiInfo { Title = "HoneyComb", Version = "v2" });
        c.TagActionsBy(api =>
        {
            RouteAttribute? routeMetadata = (RouteAttribute?)api.ActionDescriptor.EndpointMetadata
                .FirstOrDefault(m => m is RouteAttribute);

            return new List<string> { routeMetadata?.Name ?? "Default" };
        });
        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    })
    .AddEndpointsApiExplorer()
    .RegisterCommands(new[] { typeof(InvokeHttpRequest).Assembly })
    .AddFabron()
    .AddSingleton<IReminderManager, ReminderManager>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
builder.Services.AddHealthChecks();


if (builder.Environment.IsEnvironment("Localhost"))
{
    builder.Services
        .UseInMemoryJobQuerier();
    builder.Services.AddSingleton<IReminderQuerier, NoopReminderQuerier>();
}
else
{
    builder.Services.Configure<PostgreSQLOptions>(builder.Configuration.GetSection("PostgreSQL"));
    builder.Services
        .AddPostgreSQLIndexStore();
    builder.Services.AddSingleton<IReminderQuerier, ReminderQuerier>();
}

WebApplication? app = builder.Build();

app
    .UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "HoneyComb v2");
        c.DisplayOperationId();
    })
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();

app.MapHealthChecks("/health")
    .AllowAnonymous();
app.MapControllers();

app.MapAppsEndpoints();
app.MapPost("/stub", async c =>
{
    await c.Response.WriteAsJsonAsync(new
    {
        RequestBody = await c.Request.ReadFromJsonAsync<object>(),
        c.Request.Headers,
    });
});

app.Run();
