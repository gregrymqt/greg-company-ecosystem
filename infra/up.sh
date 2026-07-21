#!/usr/bin/env bash

# ==============================================================================
# GREG COMPANY ECOSYSTEM - AUTOMATED BUILD & KUBERNETES DEPLOYMENT
# ==============================================================================
set -e # Interrompe o script imediatamente se algum comando falhar

# Cores para o output no terminal
GREEN="\033[0;32m"
CYAN="\033[0;36m"
YELLOW="\033[1;33m"
RED="\033[0;31m"
RESET="\033[0m"

# 1. Garante que o script está rodando da raiz do projeto
if [ ! -d "backend" ] || [ ! -d "ecommerce-bot" ] || [ ! -d "go-worker" ]; then
    echo -e "${RED}✕ Erro: Execute o script a partir da raiz do repositório (greg-company-ecosystem).${RESET}"
    exit 1
fi

MANIFESTS_DIR="infra/manifests"

echo -e "${CYAN}======================================================================${RESET}"
echo -e "${CYAN}🔨 [ETAPA 1] GERANDO AS IMAGENS DOCKER NA VPS${RESET}"
echo -e "${CYAN}======================================================================${RESET}"

# Build da API C# (.NET 8)
echo -e "\n${YELLOW}📦 [1/3] Compilando e gerando imagem: infra-backend:latest...${RESET}"
docker build -t infra-backend:latest ./backend

# Build do Bot Python (FastAPI / AI)
echo -e "\n${YELLOW}📦 [2/3] Compilando e gerando imagem: ecommerce-bot:latest...${RESET}"
docker build -t ecommerce-bot:latest ./ecommerce-bot

# Build do Go Worker (Email / Transcoding)
echo -e "\n${YELLOW}📦 [3/3] Compilando e gerando imagem: infra-go-worker:latest...${RESET}"
docker build -t infra-go-worker:latest ./go-worker

# Tratamento para K3s / MicroK8s (se a VPS não usar o Docker Daemon nativo do K8s)
if command -v k3s &> /dev/null; then
    echo -e "\n${YELLOW}🔄 Importando imagens compiladas para o runtime do K3s...${RESET}"
    docker save infra-backend:latest | k3s ctr images import -
    docker save ecommerce-bot:latest | k3s ctr images import -
    docker save infra-go-worker:latest | k3s ctr images import -
elif command -v microk8s &> /dev/null; then
    echo -e "\n${YELLOW}🔄 Importando imagens compiladas para o runtime do MicroK8s...${RESET}"
    docker save infra-backend:latest | microk8s ctr image import -
    docker save ecommerce-bot:latest | microk8s ctr image import -
    docker save infra-go-worker:latest | microk8s ctr image import -
fi

echo -e "\n${CYAN}======================================================================${RESET}"
echo -e "${CYAN}🚀 [ETAPA 2] APLICANDO MANIFESTOS NO KUBERNETES${RESET}"
echo -e "${CYAN}======================================================================${RESET}"

# 1. Namespace
echo -e "\n${YELLOW}[1/5] Aplicando Namespace...${RESET}"
kubectl apply -f "${MANIFESTS_DIR}/00-namespace.yaml"

# 2. Secrets & ConfigMaps
echo -e "\n${YELLOW}[2/5] Aplicando Secrets e ConfigMaps...${RESET}"
kubectl apply -f "${MANIFESTS_DIR}/greg-secrets.yaml"
kubectl apply -f "${MANIFESTS_DIR}/greg-configmap.yaml"

# 3. Cache Interno (Redis)
echo -e "\n${YELLOW}[3/5] Subindo Cache Interno (Redis)...${RESET}"
kubectl apply -f "${MANIFESTS_DIR}/redis-infra.yaml"

# 4. Aplicações Stateless (.NET API, Python Bot, Go Worker)
echo -e "\n${YELLOW}[4/5] Subindo Aplicações no K8s...${RESET}"
kubectl apply -f "${MANIFESTS_DIR}/apps-infra.yaml"

# 5. Ingress Controller
echo -e "\n${YELLOW}[5/5] Configurando Ingress Gateway e SSE...${RESET}"
kubectl apply -f "${MANIFESTS_DIR}/greg-ingress.yaml"

echo -e "\n${CYAN}======================================================================${RESET}"
echo -e "${CYAN}⏳ [ETAPA 3] VALIDANDO SAÚDE E ROLLOUT DOS PODS${RESET}"
echo -e "${CYAN}======================================================================${RESET}"

echo -e " ⏳ Aguardando Redis..."
kubectl rollout status deployment/redis-deployment -n greg-company --timeout=60s

echo -e " ⏳ Aguardando API .NET (Ping no Supabase)..."
kubectl rollout status deployment/backend-api-deployment -n greg-company --timeout=120s

echo -e " ⏳ Aguardando Python Bot (AsyncPG + AI Services)..."
kubectl rollout status deployment/ecommerce-bot-deployment -n greg-company --timeout=120s

echo -e " ⏳ Aguardando Go Worker (CloudAMQP Consumer)..."
kubectl rollout status deployment/go-worker-deployment -n greg-company --timeout=60s

echo -e "\n${GREEN}======================================================================${RESET}"
echo -e "${GREEN}✓ DEPLOY CONCLUÍDO COM SUCESSO EM PRODUÇÃO!${RESET}"
echo -e "${GREEN}======================================================================${RESET}\n"

kubectl get pods,services,ingress -n greg-company