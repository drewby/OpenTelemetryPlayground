This appsettings example shows how to set up the SDK for a development environment running in a Docker Compose context with containers for *Grafana*, *Loki*, and *Jaeger*.

The application uses *Serilog* to write logs to both the console and a *Loki* instance. The console logs use an output template that formats unstructured logs for developers to read.

Requests to the `/metrics` and `/health` paths are not logged. 

The application exposes liveness and readiness check endpoints at `/health/live` and `/health/ready`, respectively, and a metrics endpoint at `/metrics`.

Traces are outputted to a *Jaeger* instance. Incoming requests to `/metrics` and `/health` paths are ignored. Outgoing requests to *Loki* are also ignored.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Ops": {
    "Logging": {
      "Enabled": true,
      "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Grafana.Loki", "Serilog.Expressions" ],
      "MinimumLevel": "Information",
      "Filter": [
        {
          "Name": "ByExcluding",
          "Args": {
            "expression": "RequestPath like '/metrics%' or RequestPath like '/health%'"
          }
        }
      ],
      "WriteTo": [
        { "Name": "Console",
          "Args": {
            "outputTemplate": "[{Timestamp:HH:mm:ss} {RevHash} {Level:u3}] {Message:lj}{NewLine}{Exception}"
          } 
        },
        {
          "Name": "GrafanaLoki",
          "Args": {
            "uri": "http://loki:3100",
            "propertiesAsLabels": [
              "ServiceName",
              "ServiceInstanceId",
              "ServiceVersion",
              "level"
            ]
          }
        }
      ]
    },
    "Health": {
      "Enabled": true,
      "ReadyPath": "/health/ready",
      "LivePath": "/health/live"
    },
    "Metrics": {
      "Enabled": true,
      "MetricsPath": "/metrics"
    },
    "Tracing": {
      "Enabled": true,
      "IncomingIgnore": "/metrics, /health",
      "OutgoingIgnore": "http://loki",
      "OltpExportEndpoint": "http://jaeger:4318/v1/traces"
    }
  }
}
```