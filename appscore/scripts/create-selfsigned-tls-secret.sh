#!/bin/bash

# Script to create a self-signed TLS secret for web.localhost in Kubernetes

NAMESPACE=default
SECRET_NAME=dotnetgigs-tls
DOMAIN=web.localhost

# Generate private key and certificate
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout tls.key -out tls.crt \
  -subj "/CN=$DOMAIN/O=$DOMAIN"

# Create Kubernetes TLS secret
kubectl create secret tls $SECRET_NAME \
  --cert=tls.crt \
  --key=tls.key \
  --namespace $NAMESPACE

# Do NOT clean up local files to allow importing the certificate
# rm tls.crt tls.key

echo "Self-signed TLS secret '$SECRET_NAME' created in namespace '$NAMESPACE' for domain '$DOMAIN'."
echo "The tls.crt file is kept locally for importing into your system's trusted certificates."
