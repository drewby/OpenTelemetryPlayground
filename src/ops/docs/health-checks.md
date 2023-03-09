# Health Checks

Host environments such as Kubernetes generally define two types of health checks, Readiness and Liveness, to check the status of any application instance (container, pod, etc) in the environment.  

Readiness checks let the host environment know that your application instance is ready to serve requests. The Readiness check initially returns Not Ready or Unhealthy, but the host environment allows the application instance to continue running. The host environment will not send requests to the application instance until the Readiness check returns Ready or Healthy. 

Liveness checks provide information about the running state of the application instance. Liveness checks return Healthy if the application instance is operating normally. Liveness checks that respond with Unhealthy typically cause the host environment to stop the application instance and start a new one to replace it. Liveness checks could also return other status such as Degraded and a host environment aware of such as status could stop sending requests to the instance but allow it to run and repair itself.

Health checks can do more than return a simple Healthy or Unhealthy status. They can also return more specific information about the health of specific components or dependencies for the application instance. Those components could be anything from memory resources to a database server.

In the application seed, we want to provide both Readiness and Liveness. The application developer should be able to create new health checks and add them to the health check system without modifying the Ops code. Readiness checks need a way to run at startup and update the health check system. Health checks should run asynchronously, ideally as background services, and be queryable when the host environment probes the health endpoints.

## Implementation

There are two parts to the Health Check feature of the .NET Core WebApi seed. The first is the Health Check Extension. The second is a Startup Health Check and example implementation for the application developer. Together, the features provide both Liveness and Readiness checks in the application seed.

### Health Check Extension

The Health Check Extension uses the built-in health check features in ASP.NET. During the builder phase of app initialization, the singleton ```StartupHealthCheck``` is instantiated and registered as both a Readiness and Liveness check. There is a reason it is registered for both. If something fails during the startup process, the Readiness check will forever indicate the app is not ready. This could cause the host environment to never send requests to the application instance and never trigger the Liveness check to fail. So the ```StartupHealthCheck``` needs the ability to indicate that startup failed by turning the Liveness check to Unhealthy.

The Health Check Extension maps two endpoints which are configurable in the application settings. Typically the endpoints will be /health/ready and /health/live or use z-page notation with /readyz and /healthz. The ASP.NET Health check feature uses a Tags to filter the list of HealthChecks. The Health Check Extension looks for checks tagged with "ready" for Readiness and looks for checks tagged with "live" for Liveness. 

An application developer can add more checks to the Health Check System with the following sample code:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<CustomReadyCheck>(
            "CheckReady",
            tags: new[] { HealthTag.Ready })

builder.Services.AddHealthChecks()
    .AddCheck<CustomLiveCheck>(
            "CheckLive",
            tags: new[] { HealthTag.Live })
```

The first sample adds a new Readiness check while the second one adds a new Liveness check. 

### Startup Health Check

The ```StartupHealthCheck``` and example ```StartupBackgroundService``` make it easier for application developers to create the first Readiness check. ```StartupHealthCheck``` is already implemented in the Health Extension code and added to the Services collection as an implementation of ```IStartUpHealthCheck```.

The ```StartupBackgroundService``` requests the instance of ```IStartupHealthCheck``` in its constructor. The ```StartupBackgroundService``` has one Task which executes asynchronously on startup. Once the task completes, ```IStartupHealthCheck.StartupCompleted``` is set to true causing the Readiness endpoint to return Health. If something fails during the startup task, ```IStartupHealthCheck.StartupHealthy``` is set to false, causing the Liveness check to return Unhealthy. 

The code that executes during the startup task is very dependent on the application scenario. Some examples include initializing a data store, opening a connection to a backend service, pre-populating a local cache, or downloading additional configuration.

Below is the code included in the ```StartupBackgroundService```. The Task.Delay simulates the long-running task and should be removed by the application developer. 

```csharp
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            //perform startup tasks


            // Simulate the effect of a long-running task.
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            this.logger.LogInformation("Startup Task Completed");
            this.healthCheck.StartupCompleted = true;
        }
        catch(Exception e)
        {
            // if startup fails, mark unhealthy
            this.logger.LogError(e,"Startup Task Failed");
            this.healthCheck.StartupHealthy = false;
        }
    }
```

### Health Responses

Application developers can add multiple health checks for Readiness and Liveness endpoints. Each health check can respond with Healthy, Degraded, or Unhealthy. The aggregate response will be the worst condition all of the checks. 

The health check can include additional detail in the response. For example, a check on storage might return the available storage space available to the application. A database check may respond with the return time indicating the responsivess of the database server.

Below is an example response from a Liveness check which includes a storage health check. 

```json
{
  "status": "Healthy",
  "revision": "263c099",
  "results": {
    "StartupHealthy": {
      "status": "Healthy",
      "description": "The startup task is healthy.",
      "data": {}
    },
    "StorageHealthy": {
      "status": "Healthy",
      "description": "Storage is healthy.",
      "data": {
        "/": {
          "Root": "/",
          "Format": "lofs",
          "TotalSize": 269427478528,
          "Available": 245805834240,
          "IsReady": true
        }
      }
    }
  }
}
```

The above storage health check might be implemented with the following sample code.

```csharp
public class StorageHealthCheck: IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var isReady = true;

        var allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            try
            {
                if (drive.RootDirectory.Name=="/") 
                {  
                    isReady = isReady & drive.IsReady;
                    data.Add(drive.VolumeLabel, new {
                        Root = drive.RootDirectory.FullName,
                        Format = drive.DriveFormat,
                        TotalSize = drive.TotalSize,
                        Available = drive.TotalFreeSpace,
                        IsReady = drive.IsReady
                    });
                }
            }
            catch (IOException)
            {
                isReady = false;
            }
        }
        
        if (isReady) 
        {
            return Task.FromResult(HealthCheckResult.Healthy("Storage is healthy.", data));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage is not healthy.", data: data));
        }
    }
}
```

The storage health check would be added to the health checks list using the following code in Program.cs.

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>(
            "StorageHealthy",
            tags: new[] { HealthTag.Live })
```