using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ops.Tracing;

public static class TracingExtension
{
    public static WebApplicationBuilder AddTracing(this WebApplicationBuilder builder)
    {
        var tracingFilter = new TracingFilter(OpsConfig.Current.Tracing.IncomingIgnore, OpsConfig.Current.Tracing.OutgoingIgnore);

        builder.Services.AddOpenTelemetry().WithTracing(config =>
        {
            config
                .AddSource(OpsConfig.Current.ServiceName, "Azure.*")
                .AddProcessor<TraceProcessor>()
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(OpsConfig.Current.ServiceName,
                                OpsConfig.Current.ServiceNamespace,
                                OpsConfig.Current.ServiceVersion,
                                false,
                                OpsConfig.Current.ServiceInstanceId)
                    // .AddAttributes(new Dictionary<string, object>
                    // {
                    //     ["service.revision"] = OpsConfig.Current.RevisionHash,
                    //     ["service.name"] = OpsConfig.Current.ServiceName,
                    //     ["something"] = "else"
                    // })
                )
                .AddHttpClientInstrumentation(opts =>
                {
                    opts.FilterHttpRequestMessage = context => tracingFilter.FilterOutgoing(context);
                })
                .AddGrpcClientInstrumentation()
                .AddAspNetCoreInstrumentation(opts =>
                {
                    opts.Filter = context => tracingFilter.FilterIncoming(context);
                })
                .AddSqlClientInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    if (!String.IsNullOrWhiteSpace(OpsConfig.Current.Tracing.OltpExportEndpoint))
                    {
                        opt.Endpoint = new Uri(OpsConfig.Current.Tracing.OltpExportEndpoint);
                        opt.Protocol = OpsConfig.Current.Tracing.OtlpExportProtocol;
                    }
                });
        });
        return builder;
    }

    public static WebApplication ConfigTracing(this WebApplication app)
    {
        return app;
    }
}
