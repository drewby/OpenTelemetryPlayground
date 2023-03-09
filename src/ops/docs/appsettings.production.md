This appsettings example shows how to set up the SDK for a production environment running in a Kubernetes context with dialtone services provided by the cluster.

The application uses *Serilog* to write structured logs to the console which can be collected
by a Kubernetes and forwarded to a log repositiory.

Requests to the `/metrics` and `/health` paths are not logged. 

The application exposes liveness and readiness check endpoints at `/health/live` and `/health/ready`, respectively, and a metrics endpoint at `/metrics`.

Traces are outputted to a *Jaeger* instance. Incoming requests to `/metrics` and `/health` paths are ignored. 

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Ops": {
    "Logging": {
      "Enabled": true,
      "Using": [ "Serilog.Sinks.Console", "Serilog.Expressions" ],
      "MinimumLevel": "Warning",
      "WriteTo": [
        { "Name": "Console",
          "Args": {
            "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
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
      "OltpExportEndpoint": "http://collector.linkerd-jaeger:14250"
    }
  },
  "AllowedHosts": "*"
}
```