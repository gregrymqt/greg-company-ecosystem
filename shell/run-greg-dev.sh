#!/bin/bash

# =================================================================
# Script de Inicialização - Greg Company Dev Environment
# Desenvolvido por: Lucas Vicente
# =================================================================

echo "🚀 Iniciando ambiente de desenvolvimento (Sem BI)..."

# 1. Parar o serviço SQL Server local para liberar a porta 1433
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    echo "🪟 Windows detectado (Git Bash). Tentando parar MSSQLSERVER..."
    # Necessário rodar o Git Bash como Administrador para isso funcionar
    net stop mssqlserver 2>/dev/null || echo "⚠️  Serviço local não estava rodando ou requer Admin."
else
    echo "🌿 Linux Mint detectado. Parando mssql-server..."
    sudo systemctl stop mssql-server 2>/dev/null || echo "⚠️  Serviço local não encontrado."
fi

# 2. Limpar qualquer resquício de containers parados
echo "🧹 Limpando containers antigos..."
docker compose down

# 3. Subir apenas os serviços essenciais para o Backend
# Aqui listamos explicitamente os serviços que queremos (EXCETO o bi-dashboard)
echo "🐳 Subindo infraestrutura (SQL, Mongo, Redis, Backend)..."
docker compose up -d sql-server mongodb redis backend

echo "----------------------------------------------------------"
echo "✅ Ambiente pronto!"
echo "🔗 Backend: http://localhost:5045"
echo "🛢️  SQL Server: localhost,1433 (User: sa)"
echo "----------------------------------------------------------"
echo "Dica: Use 'docker compose logs -f backend' para ver o log do app."