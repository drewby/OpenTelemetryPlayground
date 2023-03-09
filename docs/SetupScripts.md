# oncreate.sh

This bash script automates the installation and configuration of the tools and services for the Kubernetes cluster. Here is a brief overview of the steps that the script takes:

1. Check if the specified cluster already exists; if not, create a new one using the `k3d` command and some additional parameters.
1. Merge the kubeconfig for the new cluster with the current one using the `k3d` command.
1. Wait for the cluster to be ready. Time out at two minutes.
1. Add  Helm repositories.
1. Install the `ingress-nginx`, `Prometheus`, `Jaeger`, `Grafana`, `Loki`, `Promtail`, and `Redis` using Helm, if they are not already installed.
1. Check if Dapr is already installed in the cluster; if not, initialize it using the dapr command.
1. Run the make all command, which builds and deploys the Web Todo API.
1. Apply Kubernetes manifests, including ones for the nginx ingress, static website and the Todo Web API.
1. Restart the nginx and the todoapi to ensure they have read that most recent config.
