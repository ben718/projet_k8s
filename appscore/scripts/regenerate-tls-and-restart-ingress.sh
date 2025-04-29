#!/bin/bash

# Script to regenerate self-signed TLS certificate with SAN for web.localhost
# and restart the ingress to apply the new certificate

set -e

CERT_DIR="./k8s-config/secrets"
CERT_NAME="dotnetgigs-tls"
NAMESPACE="default"
HOST="web.localhost"

echo "Generating private key..."
openssl genrsa -out ${CERT_DIR}/tls.key 2048

echo "Generating certificate signing request (CSR) config with SAN..."
cat > ${CERT_DIR}/csr.conf <<EOF
[req]
default_bits       = 2048
prompt             = no
default_md         = sha256
req_extensions     = req_ext
distinguished_name = dn

[dn]
CN = ${HOST}

[req_ext]
subjectAltName = @alt_names

[alt_names]
DNS.1 = ${HOST}
EOF

echo "Generating CSR..."
openssl req -new -key ${CERT_DIR}/tls.key -out ${CERT_DIR}/tls.csr -config ${CERT_DIR}/csr.conf

echo "Generating self-signed certificate with SAN..."
openssl x509 -req -in ${CERT_DIR}/tls.csr -signkey ${CERT_DIR}/tls.key -out ${CERT_DIR}/tls.crt -days 365 -extensions req_ext -extfile ${CERT_DIR}/csr.conf

echo "Deleting old TLS secret if exists..."
kubectl delete secret ${CERT_NAME} --namespace ${NAMESPACE} --ignore-not-found

echo "Creating new TLS secret..."
kubectl create secret tls ${CERT_NAME} --cert=${CERT_DIR}/tls.crt --key=${CERT_DIR}/tls.key --namespace ${NAMESPACE}

echo "Restarting ingress controller pods to reload the new certificate..."
kubectl rollout restart deployment ingress-nginx-controller -n ingress-nginx

echo "TLS certificate regenerated and ingress restarted successfully."
