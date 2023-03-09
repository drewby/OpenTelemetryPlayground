// Description: This code defines a StartupBackgroundService class that inherits from the BackgroundService 
// class and provides a background service for performing startup tasks. The background service executes on
// a background thread and performs the following tasks:
//
//      1. Waits for the Dapr sidecar to become available.
//      2. Retrieves a list of todos from Dapr state.
//      3. If the list of todos is not found, creates a new list of todos and saves it to Dapr state.
//      4. Logs the completion of the startup task and sets a boolean flag indicating that the startup process has completed.
//      5. If the startup process fails, logs the error and sets a boolean flag indicating that the startup process was not successful and unhealthy.

using Dapr.Client;
using Ops.Health;

/// <summary>
/// Background service for performing startup tasks.
/// </summary>
public class StartupBackgroundService : BackgroundService
{
    private readonly IStartupHealthCheck healthCheck;
    private readonly ILogger<StartupBackgroundService> logger;
    private readonly DaprClient daprClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupBackgroundService"/> class with the specified logger, health check, and Dapr client.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="healthCheck">The health check instance to use for reporting startup health.</param>
    /// <param name="daprClient">The Dapr client instance to use for interacting with state store.</param>
    public StartupBackgroundService(ILogger<StartupBackgroundService> logger, IStartupHealthCheck healthCheck, DaprClient daprClient)
    {
        this.healthCheck = healthCheck;
        this.logger = logger;
        this.daprClient = daprClient;
    }

    /// <summary>
    /// Executes the startup tasks.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to use for stopping the background service.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // wait for dapr sidecar to be ready
            await daprClient.WaitForSidecarAsync(stoppingToken);

            this.logger.LogInformation("Dapr Sidecar Is Ready");

            // get todo list from state store
            List<Todo>? todoList = null;
            todoList = await daprClient.GetStateAsync<List<Todo>>("todos", "todoList");

            // if todo list is not found, create a new one
            if (todoList == null)
            {
                this.logger.LogInformation("Creating new todo list");

                todoList = new List<Todo>();
                todoList.Add(new Todo { Id = 1, Name = "Try a new ice cream flavor.", IsComplete = false });
                todoList.Add(new Todo { Id = 2, Name = "Walk backwards for 10 minutes.", IsComplete = false });
                todoList.Add(new Todo { Id = 3, Name = "Dance like no one is watching.", IsComplete = false });

                // save todo list to state store
                await daprClient.SaveStateAsync("todos", "todoList", todoList);
            }

            this.logger.LogInformation("Startup Task Completed");
            // mark startup as completed
            this.healthCheck.StartupCompleted = true;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Startup Task Failed");
            // mark startup as not healthy
            this.healthCheck.StartupHealthy = false;
        }
    }
}
