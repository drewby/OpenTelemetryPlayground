# Documentation

## OpenTelemetry

* [OpenTelemetry Collector Configurations](../deploy/kustomize/otel/README.md) 

## Application Code

* [ToDo Web Application - Single Page Application (HTML/CSS/JS)](../src/todospa/README.md)
* [ToDo API - ASP.NET Core WebApi](../src/todoapi/README.md)
* [Ops SDK - Operational capabilities (health, metrics, logging, tracing)](../src/ops/README.md)

## Infrastructure Code

* [Nginx Configuration with Dapr Sidecar](./NginxWithDapr.md)
* [Dapr State Management and Redis](./DaprStateWithRedis.md)
* [Distributed Tracing](./DistributedTracing.md)
* Grafana Datasources and Dashboards
    * New dashboards can be added for auto provisioning at [/deploy/grafana/dashboards](../deploy/grafana/dashboards/)
    * New datasources can be added for auto provisiont at [/deploy/grafana/datasources](../deploy/grafana/datasources/)

## Setup Code
* [on-create.sh](./SetupScripts.md#oncreatesh) - Initializes the kubernetes cluster.
