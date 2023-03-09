namespace Ops.Health;

public class HealthConfig
{
    public bool Enabled { get; set; } = false;

    public string ReadyPath { get; set; } = "/healthz/ready";

    public string LivePath { get; set; } = "/healthz/live";
}

