ear
#!/bin/bash
# Script to install NGINX Ingress Controller on Kubernetes cluster

# Create namespace for ingress-nginx
kubectl create namespace ingress-nginx

# Add ingress-nginx repository
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Install ingress-nginx using Helm
helm install ingress-nginx ingress-nginx/ingress-nginx --namespace ingress-nginx

# Wait for the ingress controller pods to be ready
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=120s

# Show the service details
kubectl get svc -n ingress-nginx
