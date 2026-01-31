# ============================================================
# GREG COMPANY - BI DASHBOARD DOCKER IMAGE
# Python 3.11 + Analytics Engine
# ============================================================

FROM python:3.11-slim

# Variáveis de ambiente para otimização
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1

# Instala dependências do sistema necessárias para pyodbc e drivers SQL Server
RUN apt-get update && apt-get install -y --no-install-recommends \
    # Drivers ODBC para SQL Server
    curl \
    apt-transport-https \
    gnupg \
    unixodbc \
    unixodbc-dev \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/11/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql17 \
    # Limpeza
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Define o diretório de trabalho
WORKDIR /app

# Copia o requirements.txt do BI Dashboard primeiro (otimiza cache do Docker)
COPY bi-dashboard/requirements.txt .

# Instala as dependências Python
RUN pip install --upgrade pip && \
    pip install -r requirements.txt

# Copia apenas o código do BI Dashboard para o container
COPY bi-dashboard/ .

# Ativa o modo automático para execução não-interativa
ENV BI_AUTO_MODE=true

# Comando padrão: executa o motor de BI
CMD ["python", "-m", "src.main"]
