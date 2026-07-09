# 💼 Greg Company - Plataforma de Cursos Online

Ecossistema completo para gestão de cursos online com sistema integrado de pagamentos, assinaturas e inteligência de negócios. Plataforma Full-stack desenvolvida por Lucas Vicente De Souza, estudante de Desenvolvimento de Software Multiplataforma na FATEC.

**Features Principais:**
- 🎓 Gestão completa de cursos e vídeos (com transcodificação delegada a microserviço Go)
- 💳 Sistema de pagamentos com MercadoPago (PIX, Cartão, Assinaturas)
- 👥 Autenticação e perfis de usuário (incluindo Google Login)
- 📊 Inteligência de Negócios (BI) processada de forma unificada
- 🔔 Suporte e sistema de reclamações
- 💰 Gestão de carteira digital e transações
- ✉️ Integração com SendGrid para envio de e-mails usando Transactional Outbox
- 🤖 Automação e bots desenvolvidos em Python

## 🚀 Tecnologias e Integrações

*   **Backend (API & BI):** ASP.NET 8 (C#) para APIs RESTful e processamento de métricas e lógica de BI. O projeto principal chama-se `MeuCrudCsharp`.
*   **Microserviços (Transcodificação):** Golang (`go-worker`) para processamento assíncrono e transcodificação de vídeos.
*   **Frontends (Micro-frontends):** React com TypeScript e Vite. Dividido em dois projetos isolados: `portal` (vitrine) e `admin` (gestão).
*   **Banco de Dados:** MongoDB nativo (`MongoDB.Driver`) como banco de dados principal e *Single Source of Truth* para toda a plataforma.
*   **Cache:** Redis para caching de alta performance.
*   **Mensageria e Eventos:** RabbitMQ para mensageria assíncrona e Transactional Outbox Pattern para processamento confiável de eventos (ex: Webhooks e Emails).
*   **Gateway Proxy:** Nginx como API Gateway, servindo como única porta de entrada (Porta 80) para todo o ecossistema.
*   **Pagamentos:** Integração completa com MercadoPago (Checkout Pro, Webhooks, PIX e Assinaturas).
*   **Armazenamento de Arquivos:** Integração com Supabase Storage.
*   **Automação:** Bot de e-commerce implementado em Python (`ecommerce-bot`).
*   **Containerização:** Docker, Docker Compose e Kubernetes para orquestração da infraestrutura local e em nuvem.

## 🏗️ Arquitetura

O ecossistema adota uma arquitetura de **Monorepo**, separado em serviços independentes:

1. **Proxy Gateway (Nginx):** Roteador central que recebe todas as requisições na porta 80 e direciona para o portal, admin ou backend.
2. **Backend (C#):** Clean Architecture com Vertical Slices - cada feature (Auth, Courses, MercadoPago, Analytics, etc.) possui sua própria estrutura completa. Auto-registro de dependências via Scrutor. Utiliza o Transactional Outbox Pattern.
3. **Go Worker (Golang):** Microserviço dedicado à transcodificação de vídeos, consumindo mensagens do RabbitMQ, aliviando o Backend C# de operações intensivas de I/O de arquivos locais.
4. **Ecommerce Bot (Python):** Projeto de automação Python hospedado na raiz do repositório.
5. **Portal Frontend (React):** Aplicação voltada para o usuário final. Contém a vitrine de cursos, player de vídeos e área do aluno.
6. **Admin Frontend (React):** Painel de backoffice para gestão da plataforma.

### Padrão Vertical Slice
Cada feature (ex: `Course`, `Payment`, `Mcp`) é tratada de forma autônoma. O Backend possui tudo o que a feature precisa para funcionar, e os Frontends implementam apenas as views correspondentes.

---

## 🛠️ Como Executar

### Pré-requisitos
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js v20.x](https://nodejs.org/) (com npm ou yarn)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) ou Docker Engine/Compose
*   [Go 1.21+](https://go.dev/) (Para desenvolvimento do go-worker)
*   [Python 3.10+](https://www.python.org/) (Para desenvolvimento do ecommerce-bot)

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
# Banco de Dados
MONGO_CONNECTION_STRING=mongodb://mongodb-store:27017/GregCompanyMongo
ConnectionStrings__Redis=redis-cache:6379
ConnectionStrings__RabbitMq=amqp://guest:guest@rabbitmq:5672/
USE_REDIS=true

# MercadoPago
MercadoPago__AccessToken=your_access_token
MercadoPago__PublicKey=your_public_key
MercadoPago__WebhookSecret=your_webhook_secret

# JWT
Jwt__Key=your_secret_key

# Google Auth
Google__ClientId=your_client_id
Google__ClientSecret=your_client_secret

# E-mail (SendGrid)
SendGrid__ApiKey=your_sendgrid_key
SendGrid__FromEmail=your_email@domain.com
SendGrid__FromName=Greg Company

# Base URL para Gateway
VITE_GENERAL__BASEURL=http://localhost
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
