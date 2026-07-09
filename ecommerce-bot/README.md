# E-commerce Bot 🛒🤖

Um microserviço escalável focado em extração, processamento e enriquecimento de dados de produtos de e-commerce utilizando Inteligência Artificial (OpenAI e Gemini).

## 🏗️ Arquitetura do Sistema

O projeto evoluiu para uma arquitetura de microserviço baseada no **FastAPI**, processamento assíncrono profundo (via `asyncio` e `aio-pika`), e filas do **RabbitMQ**.

- **API Central (FastAPI):** Expõe rotas HTTP sob demanda. Possui rotas de Landing Page (`/demo`) integradas com mensageria e rotas de extração de relatórios (`/export`). Conta com dependências robustas como Rate Limiting (armazenado no MongoDB com TTL).
- **Mensageria (RabbitMQ):** Topologia com isolamento multi-tenant. Separa filas para contas de demonstração (`ecommerce_demo`) e clientes assinantes (`ecommerce_prod`). Implementa um Dead Letter Exchange (DLX) nativo garantindo resiliência de falhas estruturais.
- **ScraperWorker:** Ouve ativamente a fila do RabbitMQ em background de forma não-bloqueante à API. Realiza as estratégias de crawling/parsing (JsonLD ou Markdown/LLM Fallback), salva os produtos no banco e processa contratos externos da arquitetura C# (PascalCase).
- **ProcessorWorker:** Robô independente que busca itens recém scrapeados ou em falha no banco de dados para envio às LLMs. Executa rotinas de limpeza com proteção temporal contra I/O intensivo (Anti DB Hammering).
- **ExporterWorker:** Agente encapsulado para criar exportações paginadas e padronizadas (gerando arquivos CSV para plataformas como Shopify e Nuvemshop). Ele suporta segurança Multi-Tenant via filtragem e paginação para não estourar a memória (OOM).
- **Database (MongoDB):** Central de estado unificada. Seus índices compostos (ex: `[("tenant_id", 1), ("sku", 1)]`) garantem estrito isolamento de dados por cliente e previnem vazamentos cross-tenant.

## 🚀 Como Rodar o Projeto Localmente

### 1. Pré-requisitos
- Python 3.10+ instalado.
- Gerenciador de pacotes `uv` (opcional, porém recomendado para altíssima performance).
- Docker e Docker Compose (necessários para subir o MongoDB e o RabbitMQ).

### 2. Subindo a Infraestrutura
Inicie as dependências do ambiente via Docker. O compose levantará tanto o banco de dados quanto o broker de mensageria:
```bash
docker compose up -d
```

### 3. Configuração de Ambiente (.env)
Crie um arquivo `.env` na raiz, baseando-se nas variáveis suportadas pelo `settings.py`:
```env
# Chaves de API das Inteligências Artificiais
OPENAI_API_KEY=sk-sua-chave-aqui
GEMINI_API_KEY=sua-chave-gemini-aqui

# Conexões de Infraestrutura (Padrão para uso local com Docker Compose)
MONGO_URI=mongodb://localhost:27017
RABBITMQ_URL=amqp://guest:guest@localhost:5672/

# Alertas Críticos e Notificações (Slack / Discord)
DISCORD_WEBHOOK_URL=https://discord.com/api/webhooks/dummy
```

### 4. Instalação e Execução
O projeto utiliza o `uvicorn` sob o capô para inicializar o FastAPI e as rotinas background:

**Recomendado (com UV):**
```bash
uv venv
uv pip install -r requirements.txt
uv run python -m app.main
```

**Alternativo (com pip clássico):**
```bash
python -m venv venv
.\venv\Scripts\activate  # No Linux: source venv/bin/activate
pip install -r requirements.txt
python -m app.main
```

> **Nota:** Ao rodar, a API ficará online em `http://localhost:8000`, e o worker de Scraping será inicializado atrelado ao evento de _lifespan_ da API escutando a fila de produção sem bloquear o recebimento de requisições web.
