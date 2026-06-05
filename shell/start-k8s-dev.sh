#!/bin/bash

# Define algumas cores para deixar o log do terminal mais bonito
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # Sem Cor

echo -e "${YELLOW}======================================================${NC}"
echo -e "${GREEN}🚀 INICIANDO AMBIENTE KUBERNETES - GREG COMPANY${NC}"
echo -e "${YELLOW}======================================================${NC}"

# 1. Cria o cluster (se ele já existir, o kind apenas avisa e segue o jogo)
echo -e "\n${GREEN}[1/4] Subindo o cluster Kind...${NC}"
kind create cluster --name greg-company-cluster --config infra/kind-config.yaml

# 2. Aplica todos os manifestos de uma vez só
echo -e "\n${GREEN}[2/4] Aplicando os manifestos da infraestrutura...${NC}"
kubectl apply -f infra/manifests/

# 3. Aguarda os Pods ficarem prontos (fundamental antes de fazer o túnel)
echo -e "\n${GREEN}[3/4] Aguardando os bancos de dados iniciarem...${NC}"
echo "Isso pode levar alguns minutos se o Docker estiver baixando as imagens."
sleep 5 # Dá um tempo para o K8s registrar os objetos
kubectl wait --for=condition=ready pod --all -n greg-company --timeout=300s

# 4. Abre os túneis em background (jogando os logs para o vazio /dev/null para não sujar sua tela)
echo -e "\n${GREEN}[4/4] Abrindo túneis (Port-Forward) para o localhost...${NC}"

kubectl port-forward svc/sqlserver-service 1433:1433 -n greg-company > /dev/null 2>&1 &
PID_SQL=$! # Salva o ID do processo do túnel SQL

kubectl port-forward svc/redis-service 6379:6379 -n greg-company > /dev/null 2>&1 &
PID_REDIS=$! # Salva o ID do processo do túnel Redis

kubectl port-forward svc/mongo-service 27017:27017 -n greg-company > /dev/null 2>&1 &
PID_MONGO=$! # Salva o ID do processo do túnel Mongo

echo -e "\n${YELLOW}======================================================${NC}"
echo -e "${GREEN}✅ TUDO PRONTO!${NC}"
echo -e "O SQL Server, MongoDB e Redis estão acessíveis no seu localhost."
echo -e "Você já pode rodar o backend em C# em outro terminal."
echo -e "${YELLOW}⚠️ Pressione [CTRL+C] aqui para encerrar os túneis.${NC}"
echo -e "${YELLOW}======================================================${NC}"

# 5. Função para limpar a sujeira quando você apertar CTRL+C
cleanup() {
    echo -e "\n\n${YELLOW}Encerrando os túneis com segurança...${NC}"
    kill $PID_SQL $PID_REDIS $PID_MONGO 2>/dev/null
    echo -e "${GREEN}Túneis fechados. Ambiente finalizado!${NC}"
    exit 0
}

# Fica escutando o comando de interrupção (CTRL+C) para rodar a função cleanup
trap cleanup SIGINT SIGTERM

# Mantém o script "preso" aqui para que os túneis continuem rodando
wait