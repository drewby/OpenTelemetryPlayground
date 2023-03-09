# Dapr State Management and Redis

[State management](https://docs.dapr.io/developing-applications/building-blocks/state-management/state-management-overview/) with Dapr allows you to store and retrieve state information in a distributed, scalable way, which is useful for scenarios like keeping track of user sessions or managing shopping cart data. Redis is a popular choice for state management with Dapr because of its fast read and write times and its ability to handle high volumes of data.

The [Redis state store component](https://docs.dapr.io/reference/components-reference/supported-state-stores/setup-redis/) makes it easy to implement a simple database for state storage. It can also be replaced with other state stores using various database or storage platforms.

There are three different parts to the implementation:

1) Install Redis
2) Configure the Redis state component in Dapr
3) Use the Dapr SDK to add and retrieve state

## Redis installation

We install Redis with the helm chart and the following volues:

```yaml
global:
  redis:
    password: "notsecure"
architecture: standalone
```
This chart will deploy Redis with a password of "notsecure". Make sure to secure your Redis deployment with a strong password in a production environment.

The `standalone` architecture installs the Redis service with no extra replicas since scalability is not required for this example

## Dapr Redis State Component

The Redis state component is configured with the follow Dapr component definition:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: todos
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: redis-master.redis:6379
  - name: redisPassword
    value: notsecure
```

The component is called `todos` which will be referenced as teh state store when calling the Dapr API for state management. We use the `redis-master` service as the endpoint for the host and provide the "notsecure" password. 

## Dapr Client SDK

Saving and getting values with the C# Dapr SDK is fairly straight forward:

```csharp
await _daprClient.SaveStateAsync("todos", "myTodo", myTodo);

var myTodo = await _daprClient.GetStateAsync<Todo>("todos", "myTodo");
```

Further discussion of using the Dapr SDK for state management in the Todo API application is in the [README here](../src/todoapi/README.md#todo-storage).