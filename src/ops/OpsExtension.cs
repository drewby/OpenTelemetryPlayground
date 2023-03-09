using Ops.Health;
using Ops.Logging;
using Ops.Metrics;
using Ops.Tracing;

namespace Ops;
public static class OpsExtension
{
    public static WebApplicationBuilder AddOps(this WebApplicationBuilder builder, string[] args)
    {
        OpsConfig.Load(builder.Configuration);

        builder.AddLogging()
               .AddMetrics()
               .AddHealth()
               .AddTracing();

        return builder;
    }

    public static WebApplication ConfigOps(this WebApplication app)
    {
        app.ConfigHealth()
           .ConfigMetrics();

        return app;
    }

}
