# OpenTelemetry Collector

We are using the OpenTelemetry Operator to configure collectors 
in the cluster. The Operator looks for the `OpenTelemetryCollector`
CRD and uses these configurationes to create collectors.

## OpenTelemetry Collector Configurations

* `Sidecar.yaml` - Sidecar collector forwards to central collector
* `Shared.yaml` - Creates a central stateful collector for sampling

Each of these are configured to attribute the span indicating that
it passed through its respective pipeline.

The shared configuration includes a CORs policy to allow traces from 
the http client.

## Scenario One

Three basic policies. Sample if any match.

1. tenpercent - Probablistic sampler taking 10% of normal traces
2. errors - Sample all traces with an error or unset status
3. latency - Sample all traces that extend longer than 2 seconds

```yaml
apiVersion: opentelemetry.io/v1alpha1
kind: OpenTelemetryCollector
metadata:
  name: opentelemetry
spec:
  config: |
    exporters:
      otlphttp:
        endpoint: http://jaeger-collector.jaeger:4318
      prometheusremotewrite:
        endpoint: http://prometheus-server.prometheus/api/v1/write
        tls:
          insecure: true
    processors:
      batch:
      transform:
        trace_statements:
          - context: span
            statements:
              - set(attributes["from_shared"], "true")
      tail_sampling:
        decision_wait: 5s
        num_traces: 100
        expected_new_traces_per_sec: 10
        policies:
          [
            {
              name: tenpercent,
              type: probabilistic,
              probabilistic: {sampling_percentage: 10}
            },
            { 
              name: errors,
              type: status_code,
              status_code: {status_codes: [ERROR, UNSET]}
            },
            {
              name: latency,
              type: latency,
              latency: { threshold_ms: 2000 }
            }
          ]
    receivers:
      otlp:
        protocols:
          http:
            include_metadata: true
            cors:
              allowed_origins:
                - http://localhost
                - http://localhost:8080
              max_age: 7200
      prometheus/own_metrics:
        config:
          scrape_configs:
            - job_name: otel-collector
              scrape_interval: 10s
              static_configs:
                - targets: [0.0.0.0:8888]
    service:
      pipelines:
        traces:
          receivers: [otlp]
          processors: [transform,tail_sampling]
          exporters: [otlphttp]
        metrics:
          receivers: [otlp,prometheus/own_metrics]
          processors: []
          exporters: [prometheusremotewrite]
```

## Scenario Two

Use a composite policy. Set a rate limit of 200 spans per second (spans, not traces). 
Assign the three policies from Scenario One (errors, latency, tenpercent). Allocating
at least 50% of the rate limit to errors, 25% to latency and the remainder to tenpercent.

*Note its likely just as good to sample all for the third category instead of tenpercent. Need to think on this.*

```yaml
apiVersion: opentelemetry.io/v1alpha1
kind: OpenTelemetryCollector
metadata:
  name: opentelemetry
spec:
  config: |
    exporters:
      otlphttp:
        endpoint: http://jaeger-collector.jaeger:4318
      prometheusremotewrite:
        endpoint: http://prometheus-server.prometheus/api/v1/write
        tls:
          insecure: true
    processors:
      batch:
      transform:
        trace_statements:
          - context: span
            statements:
              - set(attributes["from_shared"], "true")
      tail_sampling:
        decision_wait: 5s
        num_traces: 100
        expected_new_traces_per_sec: 10
        policies:
          [
            {
              name: composite-policy,
              type: composite,
              composite:
                {
                  max_total_spans_per_second: 200,
                  policy_order: [errors, latency, tenpercent],
                  composite_sub_policy:
                    [
                      {
                        name: errors,
                        type: status_code,
                        status_code: {status_codes: [ERROR, UNSET]}
                      },
                      {
                        name: latency,
                        type: latency,
                        latency: { threshold_ms: 2000 }
                      },
                      {
                        name: tenpercent,
                        type: probabilistic,
                        probabilistic: {sampling_percentage: 10}
                      }
                    ],
                  rate_allocation:
                    [
                      {
                        policy: errors,
                        percent: 50
                      },
                      {
                        policy: latency,
                        percent: 25
                      }
                    ]
                }
            }
          ]
    receivers:
      otlp:
        protocols:
          http:
            include_metadata: true
            cors:
              allowed_origins:
                - http://localhost
                - http://localhost:8080
              max_age: 7200
      prometheus/own_metrics:
        config:
          scrape_configs:
            - job_name: otel-collector
              scrape_interval: 10s
              static_configs:
                - targets: [0.0.0.0:8888]
    service:
      pipelines:
        traces:
          receivers: [otlp]
          processors: [transform,tail_sampling]
          exporters: [otlphttp]
        metrics:
          receivers: [otlp,prometheus/own_metrics]
          processors: []
          exporters: [prometheusremotewrite]
```
