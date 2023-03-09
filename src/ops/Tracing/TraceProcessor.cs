using System.Diagnostics;
using OpenTelemetry;

namespace Ops.Tracing
{
    public class TraceProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            activity.SetTag("service.revision", OpsConfig.Current.RevisionHash);
            activity.SetTag("service.name", OpsConfig.Current.ServiceName);
            activity.SetTag("service.namespace", OpsConfig.Current.ServiceNamespace);
            activity.SetTag("service.instanceid", OpsConfig.Current.ServiceInstanceId);
        }
    }
}