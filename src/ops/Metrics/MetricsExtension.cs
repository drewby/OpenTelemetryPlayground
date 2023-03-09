// using Prometheus;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Instrumentation.AspNetCore;
namespace Ops.Metrics;

public static class MetricsExtension
{
    public static WebApplicationBuilder AddMetrics(this WebApplicationBuilder builder)
    {
        if (OpsConfig.Current.Metrics.Enabled)
        {
            builder.Services.AddOpenTelemetry().WithMetrics(
                (builder) =>
                {
                    builder.AddPrometheusExporter()
                           .AddRuntimeInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddAspNetCoreInstrumentation();
                }
            );
        }
        return builder;
    }
    public static WebApplication ConfigMetrics(this WebApplication app)
    {
        if (OpsConfig.Current.Metrics.Enabled)
        {
            app.UseOpenTelemetryPrometheusScrapingEndpoint(OpsConfig.Current.Metrics.MetricsPath);

            // app.MapMetrics(OpsConfig.Current.Metrics.MetricsPath);
            // app.UseHttpMetrics();

            // var staticLabels = new Dictionary<string, string> {
            //     { "service_name", OpsConfig.Current.ServiceName },
            //     { "service_namespace", OpsConfig.Current.ServiceNamespace },
            //     { "service_instance_id", OpsConfig.Current.ServiceInstanceId },
            //     { "service_version", OpsConfig.Current.ServiceVersion },
            //     { "revision_hash", OpsConfig.Current.RevisionHash }
            // };

            // Prometheus.Metrics.DefaultRegistry.SetStaticLabels(staticLabels);
        }
        return app;
    }
}
