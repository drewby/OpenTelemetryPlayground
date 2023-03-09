namespace Ops.Metrics;

public class MetricsConfig
{
    public bool Enabled { get; set; } = false;

    public string MetricsPath { get; set; } = "/metrics";
}
