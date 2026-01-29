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
Motor de inteligência de negócios para análise de métricas da plataforma de cursos, seguindo padrões rigorosos de Clean Code e Separação de Responsabilidades.

---

### 🐍 Arquitetura do BI-Dashboard (Python)

**Estrutura de Pastas e Responsabilidades:**

- **controllers/**: Orquestra o fluxo ETL, coordenando extração de dados da API do sistema transacional e acionamento dos serviços de processamento.

- **services/**: Camada de inteligência de negócio. Processa métricas como:
  - 💰 Receita total e MRR (Monthly Recurring Revenue)
  - 📈 Taxa de conversão de assinaturas
  - ⚠️ Análise de chargebacks e transações falhadas
  - 👥 Engajamento de alunos por curso
  - 📊 Status de assinaturas (Ativas, Canceladas, Inadimplentes)

- **models/**: Define entidades de dados (Subscription, Transaction, Course, Chargeback) garantindo tipagem e consistência durante o processamento.

- **data/**: Centraliza exporters para APIs externas (RowsExporter, NotionAPI) para envio de dashboards executivos.

- **views/**: Formata saída de dados para terminal, Excel e estruturação de tabelas para Rows.com/Notion.

- **interfaces/**: Contratos abstratos (IDataService, IProductExporter) garantindo inversão de dependência.

- **enums/**: Status padronizados (ProductStatus: OK, CRITICO, ESGOTADO, REPOR).

---

### 📊 Fluxo de Dados (ETL)

1. **Extração**: Script Python consome dados do backend (.NET) via API REST - assinaturas, pagamentos, cursos, chargebacks.

2. **Transformação**: `DataService` processa:
   - Cálculo de MRR e churn rate
   - Agregação de receitas por plano
   - Identificação de assinaturas em risco
   - Análise de padrões de consumo de cursos

3. **Carga**: Dados processados exportados para:
   - **Rows.com**: Dashboards executivos em tempo real
   - **Notion**: Documentação de métricas e KPIs
   - **Terminal/Excel**: Relatórios locais para análise

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

### 5. Execute o Módulo de BI
```bash
cd ../../bi-dashboard # a partir da pasta frontend
pip install -r requirements.txt
python src/main.py
```

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
├── bi-dashboard/                  # Python BI Engine
│   └── src/
│       ├── controllers/           # ETL orchestration
│       ├── services/              # Business intelligence logic
│       ├── models/                # Data entities
│       ├── data/                  # API exporters (Rows, Notion)
│       ├── views/                 # Output formatters
│       └── main.py                # CLI menu
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

- **Auth**: `/api/auth/login`, `/api/auth/register`, `/api/auth/google`
- **Courses**: `/api/courses`, `/api/courses/{id}/videos`
- **Plans**: `/api/plans`, `/api/plans/{id}`
- **Subscriptions**: `/api/subscriptions/my`, `/api/subscriptions/cancel`
- **Payments**: `/api/payment/preference`, `/api/payment/webhook`
- **Wallet**: `/api/wallet/cards`, `/api/wallet/add-card`
- **Transactions**: `/api/transactions/history`
- **Chargebacks**: `/api/chargebacks`
- **Claims**: `/api/claims`, `/api/claims/{id}/messages`

Documentação completa disponível em `/swagger` após iniciar o backend.

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
