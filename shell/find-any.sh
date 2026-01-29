#!/bin/bash

# --- 1. CONFIGURAÇÕES ---
ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
BASE_PATH="$ROOT_DIR/system-app/frontend/src"
LOG_PATH="$ROOT_DIR/logs"
FILE_LOG="$LOG_PATH/any_types_found.log"

mkdir -p "$LOG_PATH"
echo "--- Busca de tipos 'any' filtrada: $(date) ---" > "$FILE_LOG"

# --- 2. EXECUÇÃO ---
echo "🔍 Analisando arquivos TypeScript (ignorando comentários) em $BASE_PATH..."

# -E: Habilita Regex Estendida
# Buscamos especificamente por ': any', 'as any', '<any' ou 'any[]'
# Depois usamos o 'grep -v' para remover linhas que começam com // ou *
grep -rE "(:[[:space:]]*any|as[[:space:]]*any|<any|any\[\])" --include="*.ts" --include="*.tsx" "$BASE_PATH" | \
grep -vE "^[[:space:]]*(\/\/|\*)" | \
while read -r line; do
    # Adiciona o número da linha e o caminho ao log
    echo "Pendente: $line" >> "$FILE_LOG"
done

echo "✨ Busca concluída! Verifique os resultados reais em: $FILE_LOG"