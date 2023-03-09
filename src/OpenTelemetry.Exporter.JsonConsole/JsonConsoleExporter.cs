using System.Text.Json;
using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.JsonConsole;

public class JsonConsoleLogRecordExporter : BaseExporter<LogRecord>
{
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        foreach (var logRecord in batch)
        {
            Console.WriteLine(JsonSerializer.Serialize(logRecord));
        }

        return ExportResult.Success;
    }
}