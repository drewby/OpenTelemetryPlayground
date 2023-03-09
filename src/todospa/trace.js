import * as config from './config.js';
import { BatchSpanProcessor } from '@opentelemetry/sdk-trace-base';

import { getWebAutoInstrumentations } from '@opentelemetry/auto-instrumentations-web';
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { Resource } from '@opentelemetry/resources';
import { SemanticResourceAttributes } from '@opentelemetry/semantic-conventions';
import * as api from "@opentelemetry/api";

const provider = new WebTracerProvider({
  resource: new Resource({
    [SemanticResourceAttributes.SERVICE_NAME]: config.SERVICE_NAME
  })
});

provider.addSpanProcessor(new BatchSpanProcessor(new OTLPTraceExporter({
  url: config.OTEL_URL,
  headers: {},
})));

provider.register({
  // Changing default contextManager to use ZoneContextManager - supports asynchronous operations - optional
  contextManager: new ZoneContextManager(),
});

// Registering instrumentations
registerInstrumentations({
  instrumentations: [
    getWebAutoInstrumentations({
      // load custom configuration for xml-http-request instrumentation
      '@opentelemetry/instrumentation-xml-http-request': {
        clearTimingResources: true,
      },
      '@opentelemetry/instrumentation-user-interaction': {
        enabled: false
      }
    })
  ],
});

const tracer = provider.getTracer(config.SERVICE_NAME);
export { tracer, api };
