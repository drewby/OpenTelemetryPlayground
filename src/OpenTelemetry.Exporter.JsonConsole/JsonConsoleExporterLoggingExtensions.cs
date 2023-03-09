using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.JsonConsole;

public static class JsonConsoleExporterLoggingExtensions
{
    public static OpenTelemetryLoggerOptions AddJsonConsoleExporter(this OpenTelemetryLoggerOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.AddProcessor(new SimpleLogRecordExportProcessor(new JsonConsoleLogRecordExporter()));
        return options;
    }
}