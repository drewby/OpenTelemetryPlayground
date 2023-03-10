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
                    options.IncludeFormattedMessage = true;
                    options.ParseStateValues = true;
                    options.AddJsonConsoleExporter();
                }
            );
        }

        return builder;
    }
}
