# ToDo API
This is a simple ASP.NET Core Web API for a ToDo application. 

A `ToDo` has an `Id` (unique integer), `Name` (string), and `IsComplete` (boolean) field.

# Architecture
The service follows a typical MVC approach for an ASP.NET WebApi. 
It uses Dapr state management which allows the storage of ToDos to be pluggable. 
It has two controllers: one for managing ToDo items and anoter for helping with Authentication.

# API Endpoints
The API is available via the Swagger OpenAPI interface at `/swagger/index.html`. 

Todo:
* `GET /api/Todo`: retrieves a list of all ToDo items
* `POST /api/Todo`: creates a new ToDo item
* `GET /api/Todo/{id}`: retrieves a specific ToDo item by its id
* `PUT /api/Todo/{id}`: updates a specific ToDo item by its id
* `DELETE /api/Todo/{id}`: deletes a specific ToDo item by its id

## ToDo Storage
CRUD operations for the ToDo list use the [Dapr state management](https://docs.dapr.io/developing-applications/building-blocks/state-management/state-management-overview/).

The whole ToDo list is retrieved, updated, and stored as a `List<Todo>` object. Currently, there is no seperation ot ToDos by user or concurrency mechanisms. Both can be added using a user ID to store the state and an Etag for optimistic concurrency.

Initialization of the initial list happens in the `StartupBackgroundService`. This plugs into the Ops SDK which is a seperate library implementing readiness and liveness health checks. 
The `StartupBackgroundService` does the following:

1. Waits for the Dapr sidecar to become available.
2. Retrieves a list of ToDos from Dapr state.
3. If the list of ToDos is not found, creates a new list of ToDos and saves it to Dapr state.
4. Logs the completion of the startup task and sets a boolean flag indicating that the startup process has completed.
5. If the startup process fails, logs the error and sets a boolean flag indicating that the startup process was not successful and unhealthy.

The `TodoService` class implements simple CRUD operations on the ToDo list.