# Ops SDK

The Ops SDK includes common capabilities for operating a service in a
cluster environment, including observability and health monitoring.

## App-Seed Capabilities
(more capabilities are on the backlog)
* [Container Labels for Obervability](docs/container-labels.md)
* [Structured Logging](docs/structured-logging.md)
* [Metrics](docs/metrics.md)
* [Tracing](docs/tracing.md)
* [Health Checks](docs/health-checks.md)

## Configuration

Wherever possible, configuration will be externally controlled by the 
application and platform teams. This will allow flexibility of configuration
without redeploying a new version of the application.

## Implementation

Each of the capabilities are implemented in isolated classes
and then instantiated from a shared `OpsExtension` class that adds extension
methods to `WebApplicationBuilder` and `WebApplication` in ASP.NET Core.

```csharp
builder.AddOps(args); // Adds the Ops capability services to the host container.

builder.ConfigOps(); // Allows the Ops capability services to finish final configuration.
```

Each Ops capability service is also implemented as an extension as a 
consistent model for ASP.NET. 

### Configuration

The `OpsConfig` class is responsible for loading the configuration data
model from the ASP.NET configuration system and presenting it to the
capabilty classes. The initial configuration is  defined in 
`appsettings.json` and `appsettings.Development.json`. The following 
example is a partial configuration:

```json
{
  "Ops": {
    "Logging": {
      "Enabled": true,
      "Using": [ "Serilog.Sinks.Console" ],
      "MinimumLevel": "Debug",
      "WriteTo": [
        { "Name": "Console",
          "Args": {
            "outputTemplate": "[{Timestamp:HH:mm:ss} {RevHash} {Level:u3}] {Message:lj}{NewLine}{Exception}"
          } 
        }
      ]
    }
  }
}
```

The platform team can modify these settings during deployment by mounting an
external `appsettings.json` structure in the container. Or the platform team 
can override settings using Environment variables. For example:

```bash
Ops__Logging__MinimumLevel=Warning
```

Setting the above environment variable (with double underlines as seperators)
would tell the Logging capability to only emit log messages at the "Warning" 
level and above.

There are two examples of appsettings.json here:
* [appsettings.development.json](docs/appsettings.development.md)
* [appsettings.production.json](docs/appsettings.production.md)


