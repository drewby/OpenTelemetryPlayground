using Ops.Health;
using Ops.Logging;
using Ops.Metrics;
using Ops.Tracing;

namespace Ops;


public class OpsConfig
{
    const string OPS_CONFIG_KEY = "Ops";
    const string REVISION_ENV_VAR = "REVISION";
    const byte REVHASH_LENGTH = 7;
    const string SERVICENAMESPACE_ENV_VAR = "NAMESPACE";
    const string SERVICENAME_ENV_VAR = "APPNAME";
    const string SERVICEVERSION_ENV_VAR = "VERSION";

    private static OpsConfig _current = new();
    public static OpsConfig Current => _current;

    public static void Load(ConfigurationManager config)
    {
        var configSection = config?.GetSection(OPS_CONFIG_KEY);

        if (configSection!=null)
        {
            _current = configSection.Get<OpsConfig>() ?? _current;
        }

        _current.RevisionHash = GetConfigValueOrDefault(config, REVISION_ENV_VAR, _current.RevisionHash)
                                    .PadLeft(REVHASH_LENGTH).Substring(0, REVHASH_LENGTH);
        _current.ServiceNamespace = GetConfigValueOrDefault(config, SERVICENAMESPACE_ENV_VAR, _current.ServiceNamespace);
        _current.ServiceName = GetConfigValueOrDefault(config, SERVICENAME_ENV_VAR, _current.ServiceName);
        _current.ServiceVersion = GetConfigValueOrDefault(config, SERVICEVERSION_ENV_VAR, _current.ServiceVersion);
    }

    private static string GetConfigValueOrDefault(ConfigurationManager? config, string key, string defaultValue)
    {
        var value = config?[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        } 
        else 
        {
            return value;
        }
    }

    public string RevisionHash { get; internal set; } = "*NOREV*";

    public string ServiceNamespace { get; internal set; } = "Contoso";

    public string ServiceName { get; internal set; } = "WebApi";

    public string ServiceVersion { get; internal set; } = "0.0.0";

    public string ServiceInstanceId { get; } = Guid.NewGuid().ToString();

    public LoggingConfig Logging { get; set; } = new LoggingConfig();

    public HealthConfig Health { get; set; } = new HealthConfig();

    public MetricsConfig Metrics { get; set; } = new MetricsConfig();

    public TracingConfig Tracing { get; set; } = new TracingConfig();
}
