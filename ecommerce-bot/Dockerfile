# Usa uma imagem oficial leve do Python
FROM python:3.12-slim

# Define o diretório de trabalho dentro do container
WORKDIR /app

# Copia o arquivo de dependências
COPY requirements.txt .

# Instala as dependências, limpando o cache do pip para otimizar o tamanho da imagem
RUN pip install --no-cache-dir -r requirements.txt

# Copia o resto do código da aplicação
COPY . .

# Comando de inicialização padrão do worker (consumidor RabbitMQ)
CMD ["uvicorn", "app.main:app", "--host", "[IP_ADDRESS]", "--port", "8000"]
