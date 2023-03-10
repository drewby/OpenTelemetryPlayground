using System.Text.Json;
using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.JsonConsole;

public class JsonConsoleLogRecordExporter : BaseExporter<LogRecord>
{
    // This is the default write function used by the exporter.
    private Action<string> writeFunction;

    public JsonConsoleLogRecordExporter() : this((message) => Console.WriteLine(message))
    {
    }

    public JsonConsoleLogRecordExporter(Action<string> writeFunction)
    {
        this.writeFunction = writeFunction;
    }

    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        foreach (var logRecord in batch)
        {
            try
            {
                dynamic jsonLogRecord = new System.Dynamic.ExpandoObject();

                jsonLogRecord.Timestamp = logRecord.Timestamp;
                jsonLogRecord.LogLevel = logRecord.LogLevel.ToString();

                if (logRecord.FormattedMessage != default)
                {
                    jsonLogRecord.Message = logRecord.FormattedMessage;
                }

                if (logRecord.CategoryName != default)
                {
                    jsonLogRecord.CategoryName = logRecord.CategoryName;
                }

                if (logRecord.EventId != default)
                {
                    jsonLogRecord.EventId = logRecord.EventId.Id;
                    if (logRecord.EventId.Name != default)
                    {
                        jsonLogRecord.EventName = logRecord.EventId.Name;
                    }
                }

                if (logRecord.Exception != default)
                {
                    jsonLogRecord.Exception = logRecord.Exception?.ToString();
                }

                if (logRecord.TraceId != default)
                {
                    jsonLogRecord.TraceId = logRecord.TraceId.ToHexString();
                    jsonLogRecord.SpanId = logRecord.SpanId.ToHexString();
                    jsonLogRecord.TraceFlags = logRecord.TraceFlags.ToString();
                }

                if (logRecord.State != null)
                {
                    if (logRecord.State is IReadOnlyList<KeyValuePair<string, object>> listKvp)
                    {
                        for (int i = 0; i < listKvp.Count; i++)
                        {
                            var kvp = listKvp[i];
                            var v = kvp.Value?.ToString();
                            if (v != null)
                            {
                                ((IDictionary<string, object>)jsonLogRecord)[kvp.Key] = v;
                            }
                        }
                    }
                    else
                    {
                        ((IDictionary<string, object>)jsonLogRecord)["State"] = logRecord.State;
                    }
                }

                writeFunction(JsonSerializer.Serialize(jsonLogRecord));

            }
            catch (Exception e)
            {
                writeFunction($"Error serializing log to JSON: {e.Message}");
            }
        }

        return ExportResult.Success;
    }
}