# 📁 cmd/

**Objetivo:** Ponto de entrada (Entry point) principal do microserviço Golang.

De acordo com o padrão de layout de projetos em Go, a pasta `cmd/` contém os binários principais. Para este worker, temos apenas a aplicação principal.

**O que colocar aqui:**
- `main.go`: Onde a aplicação é inicializada, variáveis de ambiente são lidas e a conexão com RabbitMQ e Storage é startada.
- Caso o worker seja subdividido em micro-aplicativos no futuro (ex: `cmd/video-worker`, `cmd/email-worker`), eles seriam subpastas aqui.

**Regras:**
- **Não** coloque regras de negócios ou lógicas de RabbitMQ diretamente aqui. Este arquivo deve apenas orquestrar e injetar as dependências que vêm da pasta `internal/`.
