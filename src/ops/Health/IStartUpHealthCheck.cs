using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ops.Health;

public interface IStartupHealthCheck : IHealthCheck
{
    bool StartupCompleted { get; set; }
    bool StartupHealthy { get; set; }
}
