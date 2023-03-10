#!/bin/bash
CLUSTER_NAME="${CLUSTER_NAME:-k3s-default}"
REG_NAME='registry.local'
REG_PORT='5555'

if ! k3d cluster list 2>/dev/null | grep $CLUSTER_NAME; then
    k3d cluster create ${CLUSTER_NAME} \
        --registry-create $REG_NAME:0.0.0.0:$REG_PORT \
        --port "127.0.0.1:8080:80@loadbalancer" \
        --port "30000-30002:30000-30002@server:0" \
        --volume $PWD:/otel \
        --k3s-arg "--disable=traefik@server:0"
fi

k3d kubeconfig merge $CLUSTER_NAME -ds

echo -n "Waiting for cluster to be ready..."
timeout=$(($(date +%s) + 120))
until [[ $(date +%s) -gt $timeout ]]; do
  if [ -z "$(kubectl get nodes 2>&1 1>/dev/null)" ]; then
    echo "ready."
    DONE=true
    break
  fi
  echo -n "."
  sleep 0.5
done

if [ -z "$DONE" ]; then
  echo "Timed out waiting for cluster"
  exit 1
fi

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.11.0/cert-manager.yaml

helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo add prometheus https://prometheus-community.github.io/helm-charts
helm repo add open-telemetry https://open-telemetry.github.io/opentelemetry-helm-charts
helm repo add jaegertracing https://jaegertracing.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
 
if ! helm status ingress-nginx -n nginx 1>/dev/null 2>&1; then
    echo "Installing nginx into $CLUSTER_NAME cluster"
    helm install ingress-nginx ingress-nginx/ingress-nginx -n nginx --create-namespace \
        -f $PWD/deploy/helm/nginx.yaml
else
    echo "nginx already installed."
fi

if ! helm status prometheus -n prometheus 1>/dev/null 2>&1; then
    echo "Installing prometheus into $CLUSTER_NAME cluster"
    helm install prometheus prometheus/prometheus -n prometheus --create-namespace \
        -f $PWD/deploy/helm/prometheus.yaml
else
    echo "prometheus already installed."
fi

if ! helm status jaeger -n jaeger 1>/dev/null 2>&1; then
    echo "Installing jaeger into $CLUSTER_NAME cluster"
    helm install jaeger jaegertracing/jaeger -n jaeger --create-namespace \
        -f $PWD/deploy/helm/jaeger.yaml
else
    echo "jaeger already installed."
fi

if ! helm status opentelemetry-operator -n opentelemetry 1>/dev/null 2>&1; then
    echo "Installing opentelemetry-operator into $CLUSTER_NAME cluster"
    helm install opentelemetry-operator open-telemetry/opentelemetry-operator -n opentelemetry --create-namespace \
        -f $PWD/deploy/helm/opentelemetry-operator.yaml
else
    echo "opentelemetry-operator already installed."
fi

if ! helm status grafana -n grafana 1>/dev/null 2>&1; then
    echo "Installing grafana into $CLUSTER_NAME cluster"
    kubectl create namespace grafana
    kubectl create secret generic grafanalogin -n grafana --from-literal=user=admin --from-literal=password=admin
    helm install grafana grafana/grafana -n grafana --create-namespace \
        -f $PWD/deploy/helm/grafana.yaml
else
    echo "grafana already installed."
fi

if ! helm status loki -n grafana 1>/dev/null 2>&1; then
    echo "Installing loki into $CLUSTER_NAME cluster"
    helm install loki grafana/loki -n grafana --create-namespace \
        -f $PWD/deploy/helm/loki.yaml
else
    echo "loki already installed."
fi

if ! helm status promtail -n grafana 1>/dev/null 2>&1; then
    echo "Installing promtail into $CLUSTER_NAME cluster"
    helm install promtail grafana/promtail -n grafana --create-namespace \
        -f $PWD/deploy/helm/promtail.yaml
else
    echo "promtail already installed."
fi

if ! helm status redis -n redis 1>/dev/null 2>&1; then
    echo "Installing redis into $CLUSTER_NAME cluster"
    helm install redis bitnami/redis -n redis --create-namespace \
        -f $PWD/deploy/helm/redis.yaml
else
    echo "redis already installed."
fi

if [ -z "$(kubectl get pods -n dapr-system 2>/dev/null | grep dapr-operator)" ]; then
    echo "Installing dapr into cluster."
    dapr init -k --runtime-version 1.9.0
else 
    echo "dapr already installed."
fi

/usr/local/share/nvm/install.sh; nvm install --lts

make all

kubectl apply -k $PWD/deploy/kustomize/otel

kubectl apply -k $PWD/deploy/kustomize/nodeports
kubectl apply -k $PWD/deploy/kustomize/nginx
kubectl apply -k $PWD/deploy/kustomize/apps/todospa
kubectl apply -k $PWD/deploy/kustomize/apps/todoapi

kubectl rollout restart deployment/ingress-nginx-controller -n nginx
kubectl rollout restart deployment/todoapi -n apps


# don't need for localhost auth redirect, saving for later just in case
# dotnet dev-certs https --export-path devcert.pfx 
# dotnet dev-certs https --export-path devcert.crt --format pem
