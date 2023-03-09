using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Ops.Logging;

public class LoggingTraceEnricher : ILogEventEnricher
{
    private const string TraceId = "TraceId";
    private const string SpanId = "SpanId";
    private const string ParentSpanId = "ParentSpanId";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var span = OpenTelemetry.Trace.Tracer.CurrentSpan;
        if (span == null)
        {
            return;
        }
        logEvent.AddOrUpdateProperty(new LogEventProperty(TraceId, new ScalarValue(span.Context.TraceId)));
        logEvent.AddOrUpdateProperty(new LogEventProperty(SpanId, new ScalarValue(span.Context.SpanId)));
        logEvent.AddOrUpdateProperty(new LogEventProperty(ParentSpanId, new ScalarValue(span.ParentSpanId)));
    }
}
