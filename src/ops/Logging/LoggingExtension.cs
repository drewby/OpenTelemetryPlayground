// using Serilog;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter.JsonConsole;

namespace Ops.Logging;
public static class LoggingExtension
{
    const string SERILOG_CONFIG = "Ops:Logging";
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        if (OpsConfig.Current.Logging.Enabled)
        {
            builder.Logging.ClearProviders();

            builder.Logging.AddOpenTelemetry(
                (options) =>
                {
                    options.IncludeScopes = true;
                    // options.RecordException = true;
                    options.AddJsonConsoleExporter();
                }
            );



            //     var logger = new LoggerConfiguration()
            //         .ReadFrom.Configuration(builder.Configuration, SERILOG_CONFIG)
            //         .Enrich.WithProperty("ServiceVersion", OpsConfig.Current.ServiceVersion)
            //         .Enrich.WithProperty("ServiceName", OpsConfig.Current.ServiceName)
            //         .Enrich.WithProperty("ServiceNamespace", OpsConfig.Current.ServiceNamespace)
            //         .Enrich.WithProperty("ServiceInstanceId", OpsConfig.Current.ServiceInstanceId)
            //         .Enrich.WithProperty("RevisionHash", OpsConfig.Current.RevisionHash)
            //         .Enrich.With<LoggingTraceEnricher>()
            //         .CreateLogger();

            //     builder.Host.UseSerilog(logger);
        }

        return builder;
    }
}
