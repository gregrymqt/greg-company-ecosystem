# Usa uma imagem leve do Python (baseada na versão que você usa)
FROM python:3.11-slim

# Define o diretório de trabalho dentro do container
WORKDIR /app

# Copia apenas o arquivo de dependências primeiro (otimiza o cache do Docker)
COPY requirements.txt .

# Instala as dependências (Rows, Requests, Dotenv, etc.)
RUN pip install --no-cache-dir -r requirements.txt

# Copia todo o conteúdo da pasta bi-dashboard para o container
COPY . .

# Variável de ambiente para garantir que os logs apareçam em tempo real
ENV PYTHONUNBUFFERED=1

# Ativa o modo automático para execução não-interativa
ENV BI_AUTO_MODE=true

# Comando para rodar o motor de BI
CMD ["python", "-m", "src.main"]
