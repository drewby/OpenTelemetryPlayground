# Metrics

Applications can collect and publish a variety of time-based numerical metrics, including counters, guages, and histogranms, through a push or pull mechanism. In this template, we will assume a pull mechanism, where an operator is periodically scraping an endpoint for metrics. It is no difficult to move to a push mechanism if the host environment requires.

## What to Measure

Deciding what to measure is particular to each application scenario. At a minimum, we will expose metrics that help monitor activity and the state of the application and runtime environment.

*Activity* - slice by Status, Method, Endpoint
* Total Request Count
* Failed Request Count 
* Requests in Progress
* Request Duration

*Application*
* Allocated Memory
* Connection pool queue length
* Exception count

*Runtime*
* CPU Usage
* Thread pool queue length

In addition to these, developers may want to add more measures to monitor response and latency of dependencies such as databases and other service calls. In some cases, these will require custom code. 

## Enriching Metrics Data

The platform team needs the ability to drill into metrics and determine the source application, version and application instance as well as any other context that will help in quickly diagnosing issues.

We will add the following labels to the metric data, consistent with labels on tracing and logging data:

* ServiceName
* ServiceNamespace
* ServiceVersion
* ServiceInstanceId
* RevisionHash

## Implementation

Prometheus-net is a portable .NET library that provides support for collecting application and system metrics and exposing them through an http endpoint. 

Add the reference to the Prometheus-net ASP.NET Core library and the HealthChecks library

```bash
dotnet add package prometheus-net.AspNetCore
dotnet add package prometheus-net.AspNetCore.HealthChecks
```

All of the Ops capabilities in the app seed are being added as their own extension to isolate the code from the rest of the application code. 

The code for the Metrics capability is in `Ops/MetricsExtension.cs` and  `Ops/MetricsConfig.cs`.

Initializing the Prometheus metrics capability happens during the configuration step of ASP.NET Core startup. 

```csharp
app.MapMetrics();
app.UseHttpMetrics();
```

The first line will create an endpoint `/metrics` the root of the ASP.NET Core web application and expose metrics for the dotnet runtime and prometheus. In the extension, we also pass a path toe `MapMetrics` to make the endpoint configurable.

Finally, we add the following static labels to the default registry so they'll be included with the metrics.  

```csharp
var staticLabels = new Dictionary<string, string> {
    { "service_name", OpsConfig.Current.ServiceName },
    { "service_namespace", OpsConfig.Current.ServiceNamespace },
    { "service_instance_id", OpsConfig.Current.ServiceInstanceId },
    { "service_version", OpsConfig.Current.ServiceVersion },
    { "revision_hash", OpsConfig.Current.RevisionHash }
};

Metrics.DefaultRegistry.SetStaticLabels(staticLabels);
```

The static labels are added to each metric line in the output. 

## Future Discussion

There may be better approaches than adding static labels to every metric line. The Prometheus data model doesn't seem to support "global" labels in the scrape page, but does allow for labeling jobs. Labeling jobs at a small scale is not a problem, but in a high scale environment using automation like a Prometheus operator, labeling jobs manually is not possible. Possible solutions:

1. Find to label jobs automatically when nodes/pods are discovered by Prometheus in the cluster.
2. Strategy for a unique identifier that represents the unique combination of current labels with ability to expand in reports. For example, use ServiceInstanceId with a way to expand ot other attributes in report.

