# Nginx Ingress Configuration with Dapr Sidecar

This repository provides an example of using Nginx Ingress Controller with Dapr sidecar to route traffic to Dapr-enabled services in a Kubernetes cluster.

## Motivations for Dapr

The Dapr sidecar takes care of several challenges for this project including service discovery, encryption (mTLS), and observability. We also use the [Oauth2 Authorization Code Grant Flow](./DaprOauth2Authorization.md) component to handle authorization of incoming requests to the API. 

* **Service Discovery** - When services are published with a Dapr sidecar, they
become discoverable to all other Dapr sidecars in the cluster. 
* **Service Invocation access control** - Dapr policies allow us to control access to services or even specific operations. 
* **Secure communication** - All communication between Dapr sidecars is over mTLS. Using Dapr with Nginx lets us forward requests securely to the API service.
* **Observability** - Dapr provides metrics and traces to our monitoring system to evaluate performance and faults across the cluster.
* **Components** - Dapr has several components that offer rich features for the cluster with minimal configuration. We use an Oauth2 component with the Dapr sidecar for Nginx.

## Nginx Configuration

Nginx is installed in the cluster with a helm chart in `on-create.sh`, executed during the setup of the dev container. 

### Helm chart values

The following is the values passed to the helm cart:

```yaml
controller:
  config: 
    enable-opentracing: "true"
    jaeger-collector-host: jaeger-agent.jaeger.svc.cluster.local
    jaeger-propagation-format: w3c
  podAnnotations:
    dapr.io/enabled: "true"
    dapr.io/app-id: "ingress-nginx"
    dapr.io/app-port: "80"
    dapr.io/metrics-port: "9090"
    dapr.io/sidecar-listen-addresses: "0.0.0.0"
    dapr.io/config: "dapr-config"
```

Open tracing is enabled using the jaeger agent configured in the cluster. The `jaeger-propogation-format`
is set to `w3c` so that the `traceparent` header is properly forwarded with updated parent spans from nginx. 

Dapr is configured to listen to incoming http requests so ingress-nginx can forward traffic to the sidecar via
its service address.  

The `dapr-io/metrics-port` annotation is included, even with the default port of 9090, as a indicator for the prometheus scraper to collect metrics from both the service and the dapr sidecar.

### dapr-config.yaml

The following is configuration for Dapr in the `nginx` namespace:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Configuration
metadata:
  namespace: nginx
  name: dapr-config
spec: 
  tracing:
    samplingRate: "1"
    otel:
      endpointAddress: "jaeger-collector.jaeger:4318"
      isSecure: false
      protocol: http
```

The dapr sidecar is configured with the OTLP endpoint for Jaeger and a samplingRate of "1" to collect all
traces in this example cluster.

### Ingress master

The cluster will have multiple ingress on the same host domain (localhost), so a master/minion relationship
is configured. 

The master ingress is defined here:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ingress-master
  annotations:
    nginx.org/mergeable-ingress-type: "master"
spec:
  ingressClassName: nginx
  defaultBackend:
    service:
      name: todospa
      port:
        number: 80
  rules:
  - host: localhost
```
The default backend is an Nginx server serving static HTML and Javascript.

### Ingress for todoapi

The Todo API ingress is configured in the nginx namespace so that traffic
can be rerouted to the Nginx sidecar.

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  namespace: nginx
  name: todoapi
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /v1.0/invoke/todoapi.apps/method/api/$2
    nginx.org/mergeable-ingress-type: "minion"
spec:
  ingressClassName: nginx
  rules:
  - host: localhost
    http:
      paths:
      - path: /api(/|$)(.+)
        pathType: Prefix
        backend:
          service:
            name: ingress-nginx-dapr
            port:
              number: 3500
```

The ingress configuration rewrites the Url from anything that matches the /api endpoint
to the [invocation endpoint for Dapr](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/howto-invoke-discover-services/).

For example, `/api/Todo` will rewrite to `/v1.0/invoke/todoapi.apps/method/api/Todo`.

The request is then forwarded on to the Dapr sidecar for Nginx which then can route the invocation
using Dapr's service discovery and secure communication.