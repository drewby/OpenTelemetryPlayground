using OpenTelemetry.Exporter;

namespace Ops.Tracing;

public class TracingConfig
{
    public bool Enabled { get; set; } = false;

    public string IncomingIgnore { get; set; } = "";

    public string OutgoingIgnore { get; set; } = "";

    public string OltpExportEndpoint { get; set; } = "";

    public OtlpExportProtocol OtlpExportProtocol { get; set; } = OtlpExportProtocol.HttpProtobuf;

    public string ZipkinEndpoint { get; set; } = "";
}
