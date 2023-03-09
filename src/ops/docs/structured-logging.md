# Structured Logging

The initial requirement when developing an application is the ability to view logs at the console and quickly triage errors to identify and fix bugs. At scale, the platform team needs to collect logs with the right level of detail for broader analysis of system health. 

### Logging to Console

For console output, concise one-line descriptions at the debug and/or informational (configurable) level is appropriate. Multi-line messages with too many details not useable unless the details include an exception message. 

The approach here is to define a concise structured message similar to the following:

```
[18:03:36 f7sc4ga INF] Request starting HTTP/1.1 GET http://localhost:5164/swagger/v1/swagger.json - -
[18:03:36 f7sc4ga DBG] No candidates found for the request path '/swagger/v1/swagger.json'
[18:03:36 f7sc4ga DBG] Request did not match any endpoints
[18:03:36 f7sc4ga DBG] Connection id "0HMJKCNGP2IPD" completed keep alive response.
```

These messages are easily readible at the console. The detail includes a timestamp, the revision hash of the app, the logging level of the message and a readable description of the event.

### Logging to a Central Location

When log data is collected for analysis, more details may be required to filter and query data as well as correlate across apps to find root cause quicker. For this purpose, data should be enriched further with correlation ids, thread ids, categories, etc. Those records are more verbose so extra thought must also be put into how to trasmit and store them as a system scales.

An example record for collection looks like the following:

```json
{   
    "@t":"2022-08-03T04:39:23.9385912Z",
    "@mt":"Executing controller factory for controller {Controller} ({AssemblyName})",
    "@l":"Debug",
    "Controller":"WebApi.Controllers.WeatherForecastController",
    "AssemblyName":"WebApi",
    "EventId": {
        "Id":1,
        "Name":"ControllerFactoryExecuting" },
    "SourceContext":"Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
    "ActionId":"af529c61-fd4b-4e4b-86cd-48331b522bd4",
    "ActionName":"WebApi.Controllers.WeatherForecastController.Get (WebApi)",
    "RequestId":"0HMJL18GGKE7I:00000004",
    "RequestPath":"/WeatherForecast",
    "ConnectionId":"0HMJL18GGKE7I",
    "RevHash":"f7sc4ga"
}
```

### Configuration

As the platform teams manage an application over time, there could be varying levels of log data as well as enrichment data required depending on the context. Is the app a new deployment? Are there existing health issues in the system? 

To allow the team to adjust logging without redeploying the app, we will make as much of the configuration external as possible. 

## Implementation

Serilog is a portable .NET library that provides support for structured logging to both the console and to numerous cloud storage services. 

Add the reference to the Serilog ASP.NET Core library:

```bash
dotnet add package Serilog.AspNetCore
```

All of the Ops capabilities in the app seed are being added as their own extension to isolate the code from the rest of the application code. 

The code for the Logging capability is in `Ops/LoggingExtension.cs` and  `Ops/LoggingConfig.cs`.

During initialization of the ASP.NET core container, we can simply add Serilog to the set of services in the host container.

```csharp
builder.Host.UseSerilog();
```

However, to enable external configuration and data enrichment we need a few extra steps. 

```csharp
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "Ops:Logging")
    .Enrich.WithProperty("RevHash", revHash)
    .CreateLogger();
            
builder.Host.UseSerilog(logger);
```

The first few lines load the configuration from the ASP.NET configuration providers. We also add an additional property to the data which is a Revision hash. The reveision hash is derived from an [environment variable set when the container is built](container-labels.md) and makes it easier to determine what build of a container is the source of an event.

The external configuration below can be easily updated later without rebuilding or deploying the application. This configuration instructs Serilog to write events to the console using an output template. The template is concise and only includes information useful when observing the app running from the console.

```json
"Logging": {
    "Enabled": true,
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": "Debug",
    "WriteTo": [
        {   
            "Name": "Console",
            "Args": 
            {
              "outputTemplate": "[{Timestamp:HH:mm:ss} {RevHash} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            } 
        }
    ]
}
```

### Logging to a Central Location

There are several methods for collecting and sending data to a central location for storage and analysis. One popular method is to use an agent within the cluster to collect stdout and stderr logs from pods, nodes and the cluster itself. Another method is to send enriched data directly from the application.  

If collecting from the console, the configuration can be set so that more data is 

```json
"Logging": {
    "Enabled": true,
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": "Warning",
    "WriteTo": [
        {   
            "Name": "Console",
            "Args": 
            {
              "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
            } 
        }
    ]
}
```

Using the `RenderedCompactJsonFormatter`, the events written to console now include more data and are structured in a way that can be queried and correlated to other events in the cloud architecture. The `@m` message is fully rendered and readable at the console. 

```json
{
  "@t":"2022-08-04T00:02:03.6957572Z",
  "@m":"Request finished HTTP/1.1 GET http://localhost:5164/WeatherForecast - - - 200 - application/json;+charset=utf-8 386.5792ms",
  "@i":"791a596a",
  "ElapsedMilliseconds":386.5792,
  "StatusCode":200,
  "ContentType":"application/json; charset=utf-8",
  "ContentLength":null,
  "Protocol":"HTTP/1.1",
  "Method":"GET",
  "Scheme":"http",
  "Host":"localhost:5164",
  "PathBase":"",
  "Path":"/WeatherForecast",
  "QueryString":"",
  "HostingRequestFinishedLog":"Request finished HTTP/1.1 GET http://localhost:5164/WeatherForecast - - - 200 - application/json;+charset=utf-8 386.5792ms",
  "EventId":{"Id":2},
  "SourceContext":"Microsoft.AspNetCore.Hosting.Diagnostics",
  "RequestId":"0HMJLLI5GLBF5:00000002",
  "RequestPath":"/WeatherForecast",
  "ConnectionId":"0HMJLLI5GLBF5",
  "RevHash":"f7sc4ga"
}
```

If the end anayltics tool can render messages, `CompactJsonFormatter` can be used to output a templated message. 

```json
{   
    "@t":"2022-08-03T04:39:23.9385912Z",
    "@mt":"Executing controller factory for controller {Controller} ({AssemblyName})",
    "@l":"Debug",
    "Controller":"WebApi.Controllers.WeatherForecastController",
    "AssemblyName":"WebApi",
    "EventId": {
        "Id":1,
        "Name":"ControllerFactoryExecuting" },
    "SourceContext":"Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
    "ActionId":"af529c61-fd4b-4e4b-86cd-48331b522bd4",
    "ActionName":"WebApi.Controllers.WeatherForecastController.Get (WebApi)",
    "RequestId":"0HMJL18GGKE7I:00000004",
    "RequestPath":"/WeatherForecast",
    "ConnectionId":"0HMJL18GGKE7I",
    "RevHash":"f7sc4ga"
}
```

Finally, the platform team could choose to send messages directly to a central location using one of the many Serilog sinks. The following is an example of the Azure Application Insights sink configuration. Console messages are concise and readable while the data sent to Application Insights is enriched for analytics.

> Note that the Serilog.Sinks.ApplicationInsights nuget package needs to be added to the application before configuration is possible.

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.ApplicationInsights",
      "Serilog.Sinks.Console"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {   
        "Name": "Console",
        "Args": 
        {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {RevHash} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        } 
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "connectionString": "[your connection string here]",
          "telemetryConverter":
	    "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
        }
      }
    ]
  }
}
```

