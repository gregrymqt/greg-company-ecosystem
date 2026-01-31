# 💼 Greg Company - Plataforma de Cursos Online

Ecossistema completo para gestão de cursos online com sistema integrado de pagamentos, assinaturas e inteligência de negócios. Plataforma Full-stack desenvolvida por Lucas Vicente De Souza, estudante de Desenvolvimento de Software Multiplataforma na FATEC.

**Features Principais:**
- 🎓 Gestão completa de cursos e vídeos
- 💳 Sistema de pagamentos com MercadoPago (PIX, Cartão, Assinaturas)
- 👥 Autenticação e perfis de usuário
- 📊 Dashboard de BI para métricas de negócio
- 🔔 Suporte e sistema de reclamações
- 💰 Gestão de carteira digital e transações

<!-- Sugestão: Adicionar screenshots ou um GIF da aplicação em funcionamento torna o projeto muito mais atrativo. -->
<!-- 
## 📸 Screenshots

*(coloque aqui um screenshot da área de cursos)*
*(coloque aqui um screenshot do dashboard de assinaturas)*
*(coloque aqui um screenshot do dashboard de BI)*
-->

## 🚀 Tecnologias e Integrações

*   **Sistema Transacional (Backend):** ASP.NET 8 (C#) para APIs RESTful.
*   **Sistema Transacional (Frontend):** React com TypeScript.
*   **Banco de Dados:** SQL Server para persistência de dados.
*    **MongoDB:** Armazenamento NoSQL para dados flexíveis e documentos.
*   **Cache:** Redis para caching de alta performance.
*   **Pagamentos:** Integração completa com MercadoPago (Checkout Pro, Webhooks, PIX e Assinaturas).
*   **Jobs em Background:** Hangfire para processamento de tarefas assíncronas (ex: renovação de assinaturas).
*   **Business Intelligence (BI):** Motor de ETL desenvolvido em Python.
*   **Visualização de Dados:** Integração com APIs da Rows e Notion para dashboards executivos.
*   **Containerização:** Docker e Docker Compose para orquestração do ambiente de desenvolvimento.

## 🏗️ Arquitetura

### Sistema Principal (C# & React)
A aplicação principal foca na escalabilidade, manutenibilidade e experiência do usuário:

*   **Backend (C#):** Clean Architecture com Vertical Slices - cada feature (Auth, Courses, MercadoPago, Support, Videos) possui sua própria estrutura completa (Controllers, Services, Repositories, ViewModels). Auto-registro de dependências via Scrutor. Implementa princípios SOLID para regras de negócio complexas, autenticação JWT + OAuth, e integrações financeiras seguras.

*   **Frontend (React + TypeScript):** Arquitetura features-based espelhando o backend. Cada feature possui seus próprios componentes, hooks, services, types e styles. Clean Architecture garantindo separação de concerns - lógica de negócio em `features/`, componentes UI puros em `components/`, utilitários compartilhados em `shared/`.

*   **Features Implementadas:**
    - `auth/` - Autenticação (JWT, Google OAuth)
    - `course/` - Gestão de cursos (Admin + Allow)
    - `Videos/` - Player e gerenciamento de vídeos
    - `Payment/` - Checkout (PIX, Cartão, Preferências)
    - `Subscription/` - Gestão de assinaturas
    - `Wallet/` - Carteira digital
    - `Transactions/` - Histórico de pagamentos
    - `Chargeback/` - Gestão de estornos
    - `Claim/` - Sistema de reclamações
    - `profile/`, `support/`, `home/`, `about/`

*   **Infraestrutura:** Docker Compose orquestrando SQL Server, MongoDB, Redis e aplicação. Hangfire para jobs assíncronos (renovação de assinaturas, webhooks).

### Módulo de BI (Python)
Plataforma de inteligência de negócios com **FastAPI + WebSocket** para análise de métricas em tempo real da plataforma de cursos, seguindo **Vertical Slice Architecture** alinhada com o backend C#.

---

### 🐍 Arquitetura do BI-Dashboard (Python)

**Vertical Slice Architecture** - Organização por domínio (features):

**Core Infrastructure** (`src/core/`):
- **infrastructure/**: Componentes compartilhados
  - `database.py` - SQL Server connection (SQLAlchemy)
  - `mongo_client.py` - MongoDB connection
  - `websocket.py` - WebSocket Manager (Hub pattern similar ao SignalR)
- **enums/**: `hub_enums.py` - AppHubs enum (Claims, Financial, Subscriptions, Support)
- **websocket_server.py**: Configuração de rotas WebSocket

**Feature Slices** (`src/features/`) - Cada feature auto-contida:
- **claims/**: Analytics de disputas (repository, service, schemas, websocket_handlers)
- **financial/**: Métricas financeiras e receitas (repository, service, schemas, websocket_handlers)
- **subscriptions/**: Análise de MRR, churn rate, renovações
- **support/**: Tickets de suporte (MongoDB)
- **content/**: Métricas de cursos e vídeos
- **users/**: Análise de usuários

**API Layer** (`src/api/`):
- **main.py**: FastAPI application com REST + WebSocket
- **routes/**: REST endpoints por feature (claims_routes, financial_routes)

**Métricas Processadas:**
- 💰 Receita total e MRR (Monthly Recurring Revenue)
- 📈 Taxa de conversão e churn rate
- ⚠️ Claims ativas e faturamento em risco
- 💳 Análise de chargebacks e fraudes
- 👥 Engajamento de alunos por curso
- 📊 Status de assinaturas (Ativas, Canceladas, Inadimplentes)

---

### 📊 Fluxo de Dados (API + WebSocket Real-time)

**REST API** (Consulta sob demanda):
1. **Extração**: Features consomem dados do SQL Server/MongoDB via repositories
2. **Transformação**: Services processam:
   - Cálculo de MRR e churn rate
   - Agregação de receitas por plano
   - Identificação de claims críticas (>30 dias)
   - Análise de padrões de consumo de cursos
3. **Resposta**: Endpoints REST retornam JSON para clientes

**WebSocket Hubs** (Push em tempo real):
- **Claims Hub** (`/hubs/claims`): KPIs de disputas, alertas de claims críticas
- **Financial Hub** (`/hubs/financial`): Updates de receita, novos pagamentos
- **Subscriptions Hub**: Renovações, cancelamentos em tempo real
- **Support Hub**: Status de tickets de suporte

**Background Tasks**:
- Broadcast automático de KPIs a cada 30 segundos
- Notificações push de eventos críticos
- Sincronização periódica com Rows.com/Notion

---

## 🛠️ Como Executar

### Pré-requisitos
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js v20.x](https://nodejs.org/) (com npm ou yarn)
*   [Python 3.10+](https://www.python.org/downloads/)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Configuração do Ambiente
Clone o repositório e crie o arquivo de variáveis de ambiente.

```bash
git clone https://github.com/seu-usuario/greg-company-ecosystem.git
cd greg-company-ecosystem

# Crie um arquivo .env na raiz e adicione as chaves necessárias.
# Você pode usar o .env.example como base (recomendo criar um).
cp .env.example .env 
```

Preencha o `.env` com suas chaves de API (Rows, MercadoPago, etc.) e a string de conexão do banco.

### 2. Suba a Infraestrutura
Inicie os containers do SQL Server e Redis.

```bash
docker-compose up -d
```

### 3. Execute o Backend (API)
```bash
cd system-app/backend
dotnet run
```
A API estará disponível em `https://localhost:7035` (verifique o `launchSettings.json`). A documentação Swagger estará em `/swagger`.

### 4. Execute o Frontend (React App)
```bash
cd system-app/frontend
npm install
npm run dev  # Vite dev server na porta 5173
```
A aplicação estará rodando em `http://localhost:5173`.

### 5. Execute o Módulo de BI (FastAPI Server)
```bash
cd ../../bi-dashboard # a partir da pasta frontend
pip install -r requirements.txt
python run_api.py  # FastAPI server com WebSocket
```

A API de BI estará disponível em:
- **REST API**: `http://localhost:8000`
- **WebSocket**: `ws://localhost:8000/hubs/[hub-name]`
- **Documentação**: `http://localhost:8000/docs`
- **Status Hubs**: `http://localhost:8000/ws/status`

---

## 📂 Estrutura do Projeto

```
greg-company-ecosystem/
├── system-app/
│   ├── backend/                    # .NET 8 API
│   │   ├── Features/              # Vertical Slices (Auth, Courses, MercadoPago, etc.)
│   │   ├── Extensions/            # DI, Auth, Persistence config
│   │   ├── Data/                  # DbContext, Migrations
│   │   └── Program.cs             # Entry point
│   │
│   └── frontend/                  # React + TypeScript + Vite
│       └── src/
│           ├── features/          # Features-based (auth, course, Payment, etc.)
│           ├── components/        # UI components puros
│           ├── pages/             # Route-level pages
│           ├── shared/            # Shared utilities
│           └── routes/            # Routing config
│
├── bi-dashboard/                  # Python BI Engine (FastAPI + WebSocket)
│   └── src/
│       ├── core/                  # Shared infrastructure
│       │   ├── infrastructure/    # Database, MongoDB, WebSocket Manager
│       │   ├── enums/             # AppHubs enum
│       │   └── websocket_server.py # WebSocket routes setup
│       ├── features/              # Vertical Slices (domain-based)
│       │   ├── claims/           # repository, service, schemas, websocket_handlers
│       │   ├── financial/        # repository, service, schemas, websocket_handlers
│       │   ├── subscriptions/    # repository, service, schemas
│       │   ├── support/          # repository, service, schemas
│       │   ├── content/          # repository, schemas
│       │   └── users/            # repository, schemas
│       └── api/                   # FastAPI application
│           ├── main.py            # App + background tasks
│           └── routes/            # REST endpoints
│   └── run_api.py                 # Script to run FastAPI server
│
├── mcp-servers/                   # Model Context Protocol servers
│   ├── greg_context_mcp.py       # Architecture context for AI
│   └── log_mcp_server.py         # Log analysis for AI
│
├── docker-compose.yml             # Infrastructure orchestration
└── .env                           # Environment variables
```

---

## 🔑 Variáveis de Ambiente Necessárias

Configure no arquivo `.env`:

```env
# Database
ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=GregCompany;...

# MercadoPago
MERCADOPAGO_ACCESS_TOKEN=your_access_token
MERCADOPAGO_PUBLIC_KEY=your_public_key

# JWT
JWT_SECRET=your_secret_key
JWT_ISSUER=GregCompanyAPI
JWT_AUDIENCE=GregCompanyClient

# Redis (opcional, use USE_REDIS=false para desabilitar)
USE_REDIS=true
REDIS_CONNECTION=localhost:6379

# MongoDB
MONGODB_CONNECTION=mongodb://localhost:27017

# BI APIs
ROWS_API_KEY=your_rows_api_key
NOTION_API_KEY=your_notion_api_key
```

---

## 🎯 Endpoints Principais da API

### Sistema Transacional (.NET - Porta 7035)
- **Auth**: `/api/auth/login`, `/api/auth/register`, `/api/auth/google`
- **Courses**: `/api/courses`, `/api/courses/{id}/videos`
- **Plans**: `/api/plans`, `/api/plans/{id}`
- **Subscriptions**: `/api/subscriptions/my`, `/api/subscriptions/cancel`
- **Payments**: `/api/payment/preference`, `/api/payment/webhook`
- **Wallet**: `/api/wallet/cards`, `/api/wallet/add-card`
- **Transactions**: `/api/transactions/history`
- **Chargebacks**: `/api/chargebacks`
- **Claims**: `/api/claims`, `/api/claims/{id}/messages`

### BI Dashboard API (Python - Porta 8000)
**REST Endpoints:**
- **Claims**: `GET /api/claims/kpis`, `GET /api/claims/active`, `GET /api/claims/critical`
- **Financial**: `GET /api/financial/summary`, `GET /api/financial/revenue`
- **Status**: `GET /ws/status` - Status dos WebSocket hubs

**WebSocket Hubs:**
- **Claims**: `ws://localhost:8000/hubs/claims`
- **Financial**: `ws://localhost:8000/hubs/financial`
- **Subscriptions**: `ws://localhost:8000/hubs/subscriptions`
- **Support**: `ws://localhost:8000/hubs/support`

Documentação completa disponível em:
- Backend .NET: `/swagger` 
- BI Dashboard: `http://localhost:8000/docs`

---

## 🧪 Testes

```bash
# Backend
cd system-app/backend
dotnet test

# Frontend
cd system-app/frontend
npm run test
```

---

## 📝 Licença

Este projeto foi desenvolvido como trabalho acadêmico na FATEC - Faculdade de Tecnologia de São Paulo.

---

## 👨‍💻 Autor

**Lucas Vicente De Souza**  
Estudante de Desenvolvimento de Software Multiplataforma - FATEC

---

## 🚀 Próximas Features

- [ ] Sistema de avaliações de cursos
- [ ] Certificados digitais automáticos
- [ ] Relatórios de progresso do aluno
- [ ] Integração com outras plataformas de pagamento
- [ ] App mobile (React Native)
- [ ] Gamificação (badges, rankings)
