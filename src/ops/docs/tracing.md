# Insights

To have good observability across multiple application services, we need to collect arbituarily wide event data, correlate events, and display details across highly cardinal data dimensions. The most common correlation is a trace identifier, but other dimensions will be added over time that may include transaction ids, user ids, etc.

To begin, the following common data dimensions will be made available across logs, metrics, and trace data.

* **Cloud.RoleName** - The role that the service is playing in the broader system.           
* **Cloud.RoleInstance** - An instance identifier for the server or container sourcing the data.        
* **service.revhash** - The revision hash which will typically be derived from the source control commit which published the current version of the service. 
* **service.namespace** - A namespace to groups services together by organization or team.
* **service.name** - The descriptive name of the service or application. Note that the same service implementation could play multiple roles, so service.name is not always the same as Cloud.RoleName.
* **service.version** - The currently deployed version of the service.
* **service.instanceid** - A unique identified for the specific instance of the service. 

In addition, three levels of correlation Ids will be available in logs and trace data. These dimensions are specific to the Application Insights tooling.

* **Operation Id** - A correlation vector across a series of related traces and log messages as part of an end-to-end operation in the system. Often called a Trace Id in observability systems.
* **Id** -  A unique identifier for the current trace event. Often called a Span Id in observability systems.
* **Parent Id** -  As services call other services, the Parent Id points to the trace of calling operation (the `Id` of the parent operation). The obseravaility system can connect operations to recreate a graph of traces within the whole operation.

## Implementation

The `InsightsExtension` initializes the Application Insights SDK for trace and metric telemetry.

```csharp
builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
builder.Services.AddApplicationInsightsTelemetry(opts => {
    opts.ApplicationVersion = OpsConfig.Current.ServiceVersion;
});
builder.Services.AddApplicationInsightsTelemetryProcessor<ProbeFilter>();
```

The first line adds a `TelemetryInitializer` to the set of services. The `TelemetryInitializer` will add enrichment data to each trace so the data dimensions mentioned above are available in insights queries.

The second line adds Application Insights to the set of services used by ASP.NET core and initializes the SDK with some contextual information such as the current version of the service.

The final line adds a trace filter called `ProbeFilter` to the telemetry pipeline.

### TelemetryInitializer

The `TelemetryInitializer` has one method called `Initialize` which is called each time a new trace is created. During this step, we are setting the cloud role name and instance id, and the following attributes in Global Properties. 

```csharp
public void Initialize(ITelemetry telemetry)
{
    telemetry.Context.Cloud.RoleName = OpsConfig.Current.ServiceName;
    telemetry.Context.Cloud.RoleInstance = OpsConfig.Current.ServiceInstanceId;
    telemetry.Context.GlobalProperties["service.revhash"] = OpsConfig.Current.RevisionHash;
    telemetry.Context.GlobalProperties["service.namespace"] = OpsConfig.Current.ServiceNamespace;
    telemetry.Context.GlobalProperties["service.name"] = OpsConfig.Current.ServiceName;
    telemetry.Context.GlobalProperties["service.version"] = OpsConfig.Current.ServiceVersion;
    telemetry.Context.GlobalProperties["service.instanceid"] = OpsConfig.Current.ServiceInstanceId;
}
```

A future improvement may be to inspect if these properties are already set (a child trace created inherits the parents properties). One may choose to add more dimension data using the `HttpContext` object or other contextual information.

### ProbeFilter

The `ProbeFilter` is used to ignore any trace data that is not needed for insights analysis. For our `ProbeFilter` we are filtering trace messages for request to the Health endpoints.

```csharp
public void Process(ITelemetry item)
{
    // To filter out an item, return without calling the next processor.
    if (item.Context.Operation.Name.StartsWith($"GET {OpsConfig.Current.Health.ReadyPath}") ||
        item.Context.Operation.Name.StartsWith($"GET {OpsConfig.Current.Health.LivePath}")) { return; }

    this.Next.Process(item);
}
```
