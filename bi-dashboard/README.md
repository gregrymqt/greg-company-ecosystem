# 📊 BI Dashboard - Greg Company Ecosystem

## 🎯 Visão Geral

Dashboard de Business Intelligence para análise de dados em tempo real do ecossistema Greg Company. Utiliza **FastAPI + WebSocket** para fornecer métricas e KPIs através de APIs REST e comunicação em tempo real.

---

## 🛠️ Stack Tecnológico

- **Python 3.x**
- **FastAPI** - API REST
- **WebSocket** - Comunicação em tempo real
- **SQLAlchemy** - Acesso a SQL Server
- **PyMongo** - Acesso a MongoDB
- **Pydantic** - Validação de dados/schemas

---

## 🏗️ Padrão Arquitetural

**VERTICAL SLICE ARCHITECTURE** (alinhado com backend C#)

Cada feature é auto-contida com:
- **Repository**: Acesso a dados (SQL/MongoDB)
- **Service**: Lógica de negócio + Factory pattern
- **Schemas**: DTOs com Pydantic
- **WebSocket Handlers**: Eventos em tempo real (opcional)
- **Enums**: Tipos específicos do domínio

---

## 📂 Estrutura do Projeto

```
📁 bi-dashboard/
│
├── 📄 run_api.py                    # Ponto de entrada da aplicação
├── 📄 requirements.txt              # Dependências Python
├── 📄 Dockerfile                    # Container Docker
├── 📁 output/                       # Arquivos de saída gerados
│
└── 📁 src/                          # Código-fonte principal
    │
    ├── 📁 api/                      # Camada de API
    │   ├── main.py                  # FastAPI app + CORS + background tasks
    │   └── routes/                  # Endpoints REST
    │       ├── claims_routes.py     # Rotas de reclamações
    │       └── financial_routes.py  # Rotas financeiras
    │
    ├── 📁 core/                     # Infraestrutura compartilhada
    │   ├── websocket_server.py      # Configuração de rotas WebSocket
    │   ├── enums/
    │   │   └── hub_enums.py         # Enum AppHubs (padrão Hub)
    │   └── infrastructure/
    │       ├── database.py          # SQL Server (SQLAlchemy)
    │       ├── mongo_client.py      # Cliente MongoDB
    │       └── websocket.py         # WebSocket Manager (Hub pattern)
    │
    └── 📁 features/                 # Features (Vertical Slices)
        │
        ├── 📁 claims/               # Analytics de reclamações
        │   ├── repository.py        # Queries SQL/MongoDB
        │   ├── service.py           # Lógica + create_claims_service()
        │   ├── schemas.py           # DTOs Pydantic
        │   ├── enums.py             # Enums do domínio
        │   └── websocket_handlers.py # Handlers WebSocket
        │
        ├── 📁 financial/            # Analytics financeiro
        │   ├── repository.py
        │   ├── service.py
        │   ├── schemas.py
        │   └── websocket_handlers.py
        │
        ├── 📁 subscriptions/        # MRR, churn rate, métricas de assinatura
        │   ├── repository.py
        │   ├── service.py
        │   ├── schemas.py
        │   └── websocket_handlers.py
        │
        ├── 📁 support/              # Tickets de suporte (MongoDB)
        │   ├── repository.py
        │   ├── service.py
        │   ├── schemas.py
        │   └── websocket_handlers.py
        │
        ├── 📁 content/              # Métricas de cursos/conteúdo
        │   ├── repository.py
        │   ├── service.py           # ✨ Lógica + create_content_service()
        │   ├── schemas.py
        │   └── websocket_handlers.py # ✨ Handlers WebSocket
        │
        ├── 📁 users/                # Analytics de usuários
        │   ├── repository.py
        │   ├── service.py           # ✨ Lógica + create_users_service()
        │   ├── schemas.py
        │   └── websocket_handlers.py # ✨ Handlers WebSocket
        │
        ├── 📁 storage/              # Analytics de armazenamento
        │   ├── repository.py
        │   ├── service.py
        │   └── schemas.py
        │
        └── 📁 rows/                 # Integração com Rows.com
            ├── service.py
            └── schemas.py
```

---

## 🚀 Funcionalidades Principais

### 1️⃣ REST API
- Endpoints on-demand para consulta de dados analíticos
- Documentação automática em `/docs` (Swagger)
- Responses estruturados com Pydantic

### 2️⃣ WebSocket Hubs
- Comunicação em tempo real (padrão SignalR-like)
- Broadcasting de KPIs a cada 30 segundos
- Notificações push para clientes conectados

### 3️⃣ Background Tasks
- Tarefas periódicas para atualização de métricas
- Processamento assíncrono de dados

### 4️⃣ Multi-Database
- **SQL Server**: Dados transacionais
- **MongoDB**: Documentos flexíveis (suporte, logs)

---

## 🏃 Como Executar

### Desenvolvimento Local

#### 1. Instalar dependências
```bash
cd bi-dashboard
pip install -r requirements.txt
```

#### 2. Configurar variáveis de ambiente
Certifique-se de que o arquivo `.env` está configurado na raiz do projeto.

#### 3. Executar servidor
```bash
python run_api.py
```

#### 4. Acessar
- **REST API**: `http://localhost:8000`
- **Swagger UI**: `http://localhost:8000/docs`
- **WebSocket**: `ws://localhost:8000/hubs/[hub-name]`
- **Status Hubs**: `http://localhost:8000/ws/status`

### Docker

```bash
docker-compose up bi-engine
```

---

## ➕ Como Adicionar Nova Feature

### 1. Criar pasta em `src/features/{nome_feature}/`

### 2. Adicionar arquivos:
- `repository.py` - Acesso a dados
- `service.py` - Lógica + factory `create_{feature}_service()`
- `schemas.py` - Pydantic models
- `websocket_handlers.py` - (Opcional) Handlers WebSocket
- `enums.py` - (Opcional) Enums específicos

### 3. Criar rotas REST
Criar arquivo `src/api/routes/{feature}_routes.py`

### 4. Registrar no `src/api/main.py`
```python
from src.api.routes import {feature}_routes
app.include_router({feature}_routes.router)
```

### 5. (Opcional) Configurar handlers WebSocket
Adicione os handlers no `startup_event()` do `main.py`

✅ **Feature totalmente isolada e auto-contida!**

---

## 📐 Padrões e Convenções

### Factory Pattern
Cada service tem uma função factory para injeção de dependências:

```python
def create_{feature}_service() -> {Feature}Service:
    return {Feature}Service({Feature}Repository())
```

### Separação de Camadas
```
Repository → Service → API
```
- **Repository**: Apenas queries e acesso a dados
- **Service**: Lógica de negócio e transformações
- **API**: Recebe requests, chama service, retorna responses

### Nomenclatura
- **Arquivos**: `snake_case` (claims_routes.py)
- **Classes**: `PascalCase` (ClaimsService)
- **Funções/métodos**: `snake_case` (get_claims_data)
- **DTOs**: `PascalCase` com sufixo (ClaimsResponse, ClaimsRequest)

---

## 🔌 Conexões

### SQL Server
- Connection string em variáveis de ambiente
- SQLAlchemy para ORM
- Dados transacionais do sistema principal

### MongoDB
- URI: `mongodb://mongo-db:27017`
- Dados não-estruturados (suporte, logs)
- Queries via PyMongo

---

## 🔗 WebSocket Hubs

### Hubs Disponíveis (AppHubs enum)
- **CLAIMS_HUB**: Reclamações em tempo real
- **FINANCIAL_HUB**: Dados financeiros atualizados
- **SUBSCRIPTIONS_HUB**: Métricas de assinaturas
- **SUPPORT_HUB**: Status de tickets
- **CONTENT_HUB**: Analytics de conteúdo
- **USERS_HUB**: Dados de usuários

### Conexão WebSocket
```
ws://localhost:8000/hubs/{hub-name}
```

**Exemplo**: `ws://localhost:8000/hubs/claims`

---

## 🌐 Integração com Ecosystem

Este BI Dashboard faz parte do **Greg Company Ecosystem**:

1. **System App** (Backend .NET 8): Fonte de dados transacionais
2. **BI Dashboard** (Este projeto): ETL e analytics
3. **Rows.com/Notion**: Dashboards finais alimentados por este serviço

---

## 🐛 Troubleshooting

### Erro de Conexão com Banco
- Verificar se `docker-compose` iniciou `sql-server` e `mongodb`
- Checar variáveis de ambiente (`.env`)
- Validar connection strings

### WebSocket Não Conecta
- Confirmar que servidor está rodando
- Verificar URL (`ws://` não `wss://` em dev)
- Checar handlers registrados no `startup_event()`

### Imports Não Encontrados
- Verificar `PYTHONPATH`
- Rodar do diretório `bi-dashboard/`
- Reinstalar requirements: `pip install -r requirements.txt`

---

## 📚 Documentação Adicional

Este projeto segue as mesmas convenções do backend C# do ecosystem.  
**Mantenha a arquitetura Vertical Slice ao adicionar features.**

Para dúvidas sobre padrões arquiteturais, consulte:
- `/.github/copilot-instructions.md`
- `/mcp-servers/greg_context_mcp.py`

---

## 👨‍💻 Contato e Contribuição

**Lucas Vicente De Souza**  
Estudante de Desenvolvimento de Software Multiplataforma - FATEC

---

## 📄 Licença

Este projeto foi desenvolvido como trabalho acadêmico na FATEC - Faculdade de Tecnologia de São Paulo.
