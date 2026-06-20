# 💼 Greg Company - Plataforma de Cursos Online

Ecossistema completo para gestão de cursos online com sistema integrado de pagamentos, assinaturas e inteligência de negócios. Plataforma Full-stack desenvolvida por Lucas Vicente De Souza, estudante de Desenvolvimento de Software Multiplataforma na FATEC.

**Features Principais:**
- 🎓 Gestão completa de cursos e vídeos
- 💳 Sistema de pagamentos com MercadoPago (PIX, Cartão, Assinaturas)
- 👥 Autenticação e perfis de usuário
- 📊 Inteligência de Negócios (BI) processada de forma unificada
- 🔔 Suporte e sistema de reclamações
- 💰 Gestão de carteira digital e transações

## 🚀 Tecnologias e Integrações

*   **Backend (API & BI):** ASP.NET 8 (C#) para APIs RESTful e processamento de métricas e lógica de BI.
*   **Frontends (Micro-frontends):** React com TypeScript e Vite. Dividido em dois projetos isolados: `portal` (vitrine) e `admin` (gestão).
*   **Banco de Dados:** MongoDB nativo (`MongoDB.Driver`) como banco de dados principal e *Single Source of Truth* para toda a plataforma.
*   **Cache:** Redis para caching de alta performance.
*   **Pagamentos:** Integração completa com MercadoPago (Checkout Pro, Webhooks, PIX e Assinaturas).
*   **Jobs em Background:** Hangfire para processamento de tarefas assíncronas (ex: renovação de assinaturas).
*   **Containerização:** Docker e Docker Compose para orquestração da infraestrutura local e em nuvem.

## 🏗️ Arquitetura

O ecossistema adota uma arquitetura de **Monorepo**, separado em três aplicações primárias:

1. **Backend (C#):** Clean Architecture com Vertical Slices - cada feature (Auth, Courses, MercadoPago, Analytics, etc.) possui sua própria estrutura completa. Auto-registro de dependências via Scrutor. As regras de negócio e integrações de BI acontecem 100% neste serviço.
2. **Portal Frontend (React):** Aplicação voltada para o usuário final. Contém a vitrine de cursos, player de vídeos e área do aluno.
3. **Admin Frontend (React):** Painel de backoffice para gestão da plataforma, aprovação de reembolsos e dashboards de Analytics.

### Padrão Vertical Slice
Cada feature (ex: `Course`, `Payment`) é tratada de forma autônoma. O Backend possui tudo o que a feature precisa para funcionar, e os Frontends implementam apenas as views e integrações correspondentes à sua responsabilidade.

---

## 🛠️ Como Executar

### Pré-requisitos
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js v20.x](https://nodejs.org/) (com npm ou yarn)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Configuração do Ambiente
Clone o repositório e crie o arquivo de variáveis de ambiente.

```bash
git clone https://github.com/seu-usuario/greg-company-ecosystem.git
cd greg-company-ecosystem

# Crie um arquivo .env na raiz (baseie-se no .env.example)
cp .env.example .env 
```

### 2. Suba a Infraestrutura (Bancos e Cache)
Os arquivos centrais de orquestração estão no diretório `infra/`. Você também possui composes locais isolados em cada aplicação.
Para subir a stack completa:

```bash
cd infra
docker compose up -d
```

### 3. Execute o Backend (API)
```bash
cd backend
dotnet run
```
A API estará disponível em `http://localhost:8080` ou porta equivalente configurada. A documentação Swagger estará em `/swagger`.

### 4. Execute os Frontends (Portal e Admin)
Em terminais separados:

**Portal (Usuários):**
```bash
cd portal
npm install
npm run dev  # Acessível na porta 5173
```

**Admin (Gestores):**
```bash
cd admin
npm install
npm run dev  # Acessível na porta 5174
```

---

## 📂 Estrutura do Projeto

```text
greg-company-ecosystem/
├── backend/                   # .NET 8 API & BI Processing
│   ├── Features/              # Vertical Slices (Auth, Courses, MercadoPago, Analytics, etc.)
│   ├── Extensions/            # DI, Auth, Persistence config
│   ├── Models/                # Entidades e Value Objects
│   ├── Program.cs             # Entry point
│   ├── docker-compose.yml     # Orquestração local do backend
│   └── docker-compose.test.yml# Orquestração para a suíte de testes
│
├── portal/                    # React Micro-frontend (Usuários)
│   ├── src/features/          # Features exclusivas da visão do cliente
│   ├── docker-compose.yml     # Orquestração local do portal
│   └── package.json
│
├── admin/                     # React Micro-frontend (Gestão)
│   ├── src/features/          # Features exclusivas de dashboards e backoffice
│   ├── docker-compose.yml     # Orquestração local do admin
│   └── package.json
│
├── infra/                     # Infraestrutura Central
│   └── docker-compose.yml     # Stack de deploy (Backend + Bancos)
│
├── Tests/                     # Suíte de Testes do Backend (xUnit)
├── .github/workflows/         # Pipelines de CI/CD Isoladas
└── .env                       # Variáveis de ambiente globais
```

---

## 🔑 Variáveis de Ambiente Necessárias

Configure no arquivo `.env`:

```env
# MongoDB Connection
MONGO_DATABASE_NAME=GregCompanyMongo

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
```

---

## 🧪 Testes Unitários

O projeto possui uma suíte de testes unitários focada no backend (C#), utilizando **xUnit** e **Moq**.
Eles estão localizados na pasta `Tests/` e seguem a mesma arquitetura de *Vertical Slices* do backend.

### Como Executar

```bash
# Executar testes localmente (a partir da raiz do repositório)
dotnet test Tests/Tests.csproj

# Ou via Docker (utilizado no CI)
cd backend
docker compose -f docker-compose.test.yml up --build
```

---

## 🔄 CI/CD (Integração e Entrega Contínuas)

O projeto utiliza **GitHub Actions** para automatizar deploys. Os pipelines foram desacoplados em fluxos de trabalho independentes por serviço:

- `.github/workflows/ci-cd-backend.yml`: Monitora `backend/` e `Tests/`. Roda os testes, compila a API e publica a imagem Docker no GHCR.
- `.github/workflows/ci-cd-portal.yml`: Monitora `portal/`. Executa a compilação do TypeScript/Vite e publica a imagem.
- `.github/workflows/ci-cd-admin.yml`: Monitora `admin/`. Segue o mesmo padrão do portal.

*As imagens são publicadas com `latest` e o SHA exato do commit garantindo rastreabilidade rigorosa.*

---

## 📝 Licença
Este projeto foi desenvolvido como trabalho acadêmico na FATEC - Faculdade de Tecnologia de São Paulo.

---

## 👨‍💻 Autor
**Lucas Vicente De Souza**  
Estudante de Desenvolvimento de Software Multiplataforma - FATEC
