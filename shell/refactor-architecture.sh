#!/bin/bash

echo "========================================"
echo "REFATORACAO ARQUITETURAL - Clean Architecture"
echo "========================================"
echo ""

cd "$(dirname "$0")/src"

echo "[1/4] Movendo features orfas para src/features..."
echo ""

# Criar features se não existirem
mkdir -p features/Chargeback
mkdir -p features/Payment
mkdir -p features/Subscription
mkdir -p features/Transactions
mkdir -p features/Wallet

# Mover pastas órfãs
echo "- Movendo Chargeback..."
cp -r components/MercadoPago/Chargeback/* features/Chargeback/ 2>/dev/null || true

echo "- Movendo Payment..."
cp -r components/MercadoPago/Payment/* features/Payment/ 2>/dev/null || true

echo "- Movendo Subscription..."
cp -r components/MercadoPago/Subscription/* features/Subscription/ 2>/dev/null || true

echo "- Movendo Transactions..."
cp -r components/MercadoPago/Transactions/* features/Transactions/ 2>/dev/null || true

echo "- Movendo Wallet..."
cp -r components/MercadoPago/Wallet/* features/Wallet/ 2>/dev/null || true

echo ""
echo "[2/4] Unificando Feature Plan (Admin + Public)..."
echo ""

# Renomear plan para Plan (padronização)
if [ -d "features/plan" ]; then
    mkdir -p features/Plan
    cp -r features/plan/* features/Plan/ 2>/dev/null || true
    rm -rf features/plan
fi

# Mover lógica de Admin de MercadoPago/Plans para features/Plan
mkdir -p features/Plan/components/Admin
if [ -d "components/MercadoPago/Plans/components" ]; then
    cp -r components/MercadoPago/Plans/components/* features/Plan/components/Admin/ 2>/dev/null || true
fi

# Unificar hooks
if [ -d "components/MercadoPago/Plans/hooks" ]; then
    mkdir -p features/Plan/hooks
    cp -r components/MercadoPago/Plans/hooks/* features/Plan/hooks/ 2>/dev/null || true
fi

# Unificar services
if [ -d "components/MercadoPago/Plans/services" ]; then
    mkdir -p features/Plan/services
    cp -r components/MercadoPago/Plans/services/* features/Plan/services/ 2>/dev/null || true
fi

# Unificar types
if [ -d "components/MercadoPago/Plans/types" ]; then
    mkdir -p features/Plan/types
    cp -r components/MercadoPago/Plans/types/* features/Plan/types/ 2>/dev/null || true
fi

echo ""
echo "[3/4] Unificando Feature Claim..."
echo ""

# Mover hooks, services e types de MercadoPago/Claim para features/Claim
if [ -d "components/MercadoPago/Claim/hooks" ]; then
    mkdir -p features/Claim/hooks
    cp -r components/MercadoPago/Claim/hooks/* features/Claim/hooks/ 2>/dev/null || true
fi

if [ -d "components/MercadoPago/Claim/services" ]; then
    mkdir -p features/Claim/services
    cp -r components/MercadoPago/Claim/services/* features/Claim/services/ 2>/dev/null || true
fi

if [ -d "components/MercadoPago/Claim/types" ]; then
    mkdir -p features/Claim/types
    cp -r components/MercadoPago/Claim/types/* features/Claim/types/ 2>/dev/null || true
fi

# Mover componentes UI se existirem
if [ -d "components/MercadoPago/Claim/components" ]; then
    mkdir -p features/Claim/components
    cp -r components/MercadoPago/Claim/components/* features/Claim/components/ 2>/dev/null || true
fi

echo ""
echo "[4/4] Limpando estrutura antiga..."
echo ""

# Remover pasta MercadoPago de components
if [ -d "components/MercadoPago" ]; then
    echo "- Removendo components/MercadoPago..."
    rm -rf components/MercadoPago
fi

echo ""
echo "========================================"
echo "REFATORACAO CONCLUIDA!"
echo "========================================"
echo ""
echo "Proximos passos manuais:"
echo "1. Atualizar imports nas pages"
echo "2. Verificar duplicacoes em Plan e Claim"
echo "3. Mover componentes UI puros para shared/components se necessario"
echo "4. Executar testes"
echo ""
