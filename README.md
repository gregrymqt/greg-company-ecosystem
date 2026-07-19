# 💼 Greg Company - Plataforma de Cursos Online

Ecossistema completo para gestão de cursos online com sistema integrado de pagamentos, assinaturas e inteligência de negócios. Plataforma Full-stack desenvolvida por Lucas Vicente De Souza, estudante de Desenvolvimento de Software Multiplataforma na FATEC.

**Features Principais:**

- 🎓 Gestão completa de cursos e vídeos (com transcodificação delegada a microserviço Go)
- 💳 Sistema de pagamentos com MercadoPago (PIX, Cartão, Assinaturas)
- 👥 Autenticação e perfis de usuário (incluindo Google Login)
- 📊 Inteligência de Negócios (BI) processada de forma unificada
- 🔔 Suporte e sistema de reclamações
- 💰 Gestão de carteira digital e transações
- ✉️ Integração com SendGrid para envio de e-mails via RabbitMQ (Transactional Outbox)
- 🤖 Automação, Web Scraping e Enriquecimento de IA com Bot em Python (incluindo real-time SSE progress updates e resiliência a timeouts)

## 🚀 Tecnologias e Integrações

- **Backend (API & BI):** ASP.NET 8 (C#) para APIs RESTful e processamento de métricas e lógica de BI. O projeto principal chama-se `MeuCrudCsharp`.
- **Microserviços (Workers):** Golang (`go-worker`) para processamento assíncrono (transcodificação de vídeos e envio de e-mails).
- **Frontends (Micro-frontends):** React com TypeScript e Vite. Dividido em dois projetos isolados: `portal` (vitrine) e `admin` (gestão).
- **Banco de Dados:** PostgreSQL hospedado na nuvem Supabase (C# usa Entity Framework Core com `Npgsql`, Python usa SQLAlchemy com `asyncpg`) como banco de dados principal e *Single Source of Truth* para toda a plataforma.
- **Cache:** Redis para caching de alta performance.
- **Mensageria e Eventos:** RabbitMQ para mensageria assíncrona e Transactional Outbox Pattern para processamento confiável de eventos (ex: Webhooks e Emails).
- **Gateway Proxy:** Nginx como API Gateway, servindo como única porta de entrada (Porta 80) para todo o ecossistema.
- **Pagamentos:** Integração completa com MercadoPago (Checkout Pro, Webhooks, PIX e Assinaturas).
- **Armazenamento de Arquivos:** Integração com Supabase Storage.
- **Automação de IA:** Bot implementado em Python (`ecommerce-bot`) responsável por Web Scraping, busca semântica em Cache e enriquecimento LLM com segurança BYOK (AES-256) persistidos em PostgreSQL.
- **Containerização:** Docker e Docker Compose para ambiente local, e Kubernetes (manifestos em `infra/manifests/`) para orquestração em nuvem.

## 🏗️ Arquitetura

O ecossistema adota uma arquitetura de **Monorepo**, separado em serviços independentes:

1. **Proxy Gateway (Nginx):** Roteador central que recebe todas as requisições na porta 80 e direciona para o portal, admin ou backend.
2. **Backend (C#):** Clean Architecture com Vertical Slices - cada feature (Auth, Courses, MercadoPago, Analytics, etc.) possui sua própria estrutura completa. Auto-registro de dependências via Scrutor. Utiliza o Transactional Outbox Pattern e persistência SQL via PostgreSQL (Supabase).
3. **Go Worker (Golang):** Microserviço dedicado à transcodificação de vídeos e envio de e-mails, consumindo mensagens do RabbitMQ (`marketplace.exchange`).
4. **Ecommerce Bot (Python):** Worker de Automação de E-commerce operando Web Scraping assíncrono, processamento de NLP/LLM com isolamento Multi-tenant e criptografia de chaves (AES-256 GCM) salvas em PostgreSQL. O bot publica o progresso de extração e enriquecimento no Redis Pub/Sub e expõe um endpoint SSE (`/v1/demo/stream`) para streams em tempo real.
5. **Portal Frontend (React):** Aplicação voltada para o usuário final. Contém a vitrine de cursos, player de vídeos, área do aluno e o painel de simulação ("free-sample") resiliente a timeouts de 45s e falhas de scraping com fallback manual.
6. **Admin Frontend (React):** Painel de backoffice para gestão da plataforma, apresentando tabelas integradas de produtos e sincronização direta com logs e ações locais de retentativa e mapeamento manual.

### Padrão Vertical Slice

Cada feature (ex: `Course`, `Payment`, `Mcp`) é tratada de forma autônoma. O Backend possui tudo o que a feature precisa para funcionar, e os Frontends implementam apenas as views correspondentes.

---

## 🛠️ Como Executar

### Pré-requisitos

* [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js v20.x](https://nodejs.org/) (com npm ou yarn)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) ou Docker Engine/Compose
- [Go 1.21+](https://go.dev/) (Para desenvolvimento do go-worker)
- [Python 3.10+](https://www.python.org/) (Para desenvolvimento do ecommerce-bot)

### 1. Configuração do Ambiente

Clone o repositório e crie o arquivo de variáveis de ambiente na raiz.

```bash
git clone https://github.com/seu-usuario/greg-company-ecosystem.git
cd greg-company-ecosystem

# Crie um arquivo .env na raiz
cp .env.example .env 
```

### 2. Suba a Infraestrutura (Completa)

Os arquivos centrais de orquestração estão no diretório `infra/`. Para subir toda a stack (Bancos, Mensageria, APIs, Go Worker e Frontends com Nginx):

```bash
cd infra
docker compose up -d --build
```

A aplicação ficará disponível via `http://localhost` (Porta 80 provida pelo Nginx).

### 3. Executar o Backend Localmente (Sem Docker Compose completo)

Para rodar apenas o backend localmente no modo desenvolvimento:

```bash
cd backend
dotnet run
```

A documentação Swagger estará em `/swagger`.

### 4. Executar os Frontends Localmente (Sem Docker)

Em terminais separados:

**Portal (Usuários):**

```bash
cd portal
npm install
npm run dev
```

**Admin (Gestores):**

```bash
cd admin
npm install
npm run dev
```

---

## 📂 Estrutura do Projeto

```text
greg-company-ecosystem/
├── backend/                   # .NET 8 API & BI Processing (MeuCrudCsharp.csproj)
│   ├── Features/              # Vertical Slices (Auth, Courses, MercadoPago, Mcp, etc.)
│   ├── Extensions/            # DI, App Pipeline config
│   ├── Data/                  # Persistência
│   ├── Program.cs             # Entry point
│   ├── Dockerfile
│   └── docker-compose.yml     # Orquestração local do backend
│
├── go-worker/                 # Golang Microservice para transcodificação de vídeos
│   └── ...
│
├── ecommerce-bot/             # Automação em Python
│   └── ...
│
├── portal/                    # React Micro-frontend (Usuários)
│   ├── src/features/          # Features exclusivas da visão do cliente
│   └── Dockerfile
│
├── admin/                     # React Micro-frontend (Gestão)
│   ├── src/features/          # Features exclusivas de dashboards e backoffice
│   └── Dockerfile
│
├── infra/                     # Infraestrutura Central
│   ├── docker-compose.yml     # Stack completa (Nginx + Backend + Frontends + Bancos + RabbitMQ + Go Worker)
│   └── nginx.conf             # Regras do API Gateway
│
├── Tests/                     # Suíte de Testes
└── .env                       # Variáveis de ambiente globais
```

---

## 🔑 Variáveis de Ambiente Necessárias

Configure no arquivo `.env` na raiz do projeto:

```env
# Banco de Dados (PostgreSQL, Redis, RabbitMQ)
POSTGRES_TRANSACTION_CONNECTION_STRING="Host=postgres;Port=5432;Database=greg_company;Username=postgres;Password=postgres;Max Pool Size=40;Pooling=true;"
POSTGRES_SESSION_CONNECTION_STRING="Host=postgres;Port=5432;Database=greg_company;Username=postgres;Password=postgres;Pooling=true;"
POSTGRES_URI_PYTHON="postgresql://postgres:postgres@postgres:5432/greg_company"
POSTGRES_PASSWORD=postgres
POSTGRES_DB_NAME=greg_company
ConnectionStrings__Redis=redis:6379
USE_REDIS=TRUE
REDIS_URL=redis://redis:6379
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=
RabbitMQ__HostName=rabbitmq
RabbitMQ__Port=5672
RabbitMQ__UserName=guest
RabbitMQ__Password=guest

# MercadoPago
MercadoPago__AccessToken=your_access_token
MercadoPago__PublicKey=your_public_key
MercadoPago__WebhookSecret=your_webhook_secret

# JWT & Segurança BYOK
Jwt__Key=your_secret_key
AES_MASTER_KEY=chave_mestre_base64_aes256_32bytes

# Google Auth
Google__ClientId=your_client_id
Google__ClientSecret=your_client_secret

# E-mail (SendGrid)
SendGrid__ApiKey=your_sendgrid_key
SendGrid__FromEmail=your_email@domain.com
SendGrid__FromName=Greg Company

# Provedores de IA (ecommerce-bot)
Groq_API_KEY=gsk_sua_chave_groq_aqui
Deepseek_Api_Key=sk-sua_chave_deepseek_aqui

# Armazenamento Cloud (Supabase S3)
Access_key_ID=your_access_key
Secret_Access_key=your_secret_key
EndPoint_S3=https://sua-url-storage.supabase.co/storage/v1/s3

# Variáveis do Frontend (Vercel / Local)
VITE_GENERAL__BASEURL=http://localhost
VITE_WS_URL=ws://localhost:8000/v1/demo/stream
```

---

## 🔄 CI/CD (Integração e Entrega Contínuas)

O projeto utiliza **GitHub Actions** para automatizar deploys. Os pipelines foram desacoplados:

- `.github/workflows/ci-cd-backend.yml`
- `.github/workflows/ci-cd-portal.yml`
- `.github/workflows/ci-cd-admin.yml`

---

## 📝 Licença

Este projeto foi desenvolvido como trabalho acadêmico na FATEC - Faculdade de Tecnologia de São Paulo.

---

## 👨‍💻 Autor

**Lucas Vicente De Souza**  
Estudante de Desenvolvimento de Software Multiplataforma - FATEC
