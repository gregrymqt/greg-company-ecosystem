#!/bin/bash

# Fazer o script parar se qualquer comando falhar
set -e

echo "🚀 Destruindo cluster antigo (se houver)..."
kind delete cluster --name greg-company-cluster || true

echo "🏗️ Criando novo cluster Kind..."
kind create cluster --config kind-config.yaml

echo "🔨 Iniciando Build das imagens locais..."
echo "-> Construindo Portal..."
(cd ../portal && docker build -t infra-frontend-portal:latest .)

echo "-> Construindo Admin..."
(cd ../admin && docker build -t infra-frontend-admin:latest .)

echo "-> Construindo Backend..."
(cd ../backend && docker build -t infra-backend:latest .)

echo "-> Construindo Bot..."
(cd ../ecommerce-bot && docker build -t ecommerce-bot:latest .)

echo "-> Construindo Go Worker..."
(cd ../go-worker && docker build -t infra-go-worker:latest .)

echo "📥 Injetando imagens no cluster Kind..."
kind load docker-image infra-frontend-portal:latest --name greg-company-cluster
kind load docker-image infra-frontend-admin:latest --name greg-company-cluster
kind load docker-image infra-backend:latest --name greg-company-cluster
kind load docker-image ecommerce-bot:latest --name greg-company-cluster
kind load docker-image infra-go-worker:latest --name greg-company-cluster

echo "🌐 Instalando NGINX Ingress Controller para Kind..."
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml

echo "⏳ Aguardando os pods do Ingress ficarem prontos..."
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=90s

# 👉 ADICIONE ESTA PAUSA AQUI
echo "🔒 Dando tempo para o Webhook de Admissão estabilizar..."
sleep 15

echo "📦 Aplicando os manifestos da Greg Company..."
kubectl apply -f manifests/

echo "✅ Tudo pronto! Não esqueça de verificar os IPs com 'kubectl get svc -n greg-company'"