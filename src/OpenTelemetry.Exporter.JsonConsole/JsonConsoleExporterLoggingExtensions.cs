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

        return options.AddProcessor(new SimpleLogRecordExportProcessor(new JsonConsoleLogRecordExporter()));
    }

    public static OpenTelemetryLoggerOptions AddJsonConsoleExporter(this OpenTelemetryLoggerOptions options, Action<string> writeFunction)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return options.AddProcessor(new SimpleLogRecordExportProcessor(new JsonConsoleLogRecordExporter(writeFunction)));
    }

}