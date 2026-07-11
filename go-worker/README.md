# 📁 go-worker/

**Objetivo:** Microserviço em Golang dedicado a processamento assíncrono pesado e I/O intensivo.

O `go-worker` atua isolado da API principal (C#). Seu objetivo é livrar o Backend de tarefas que consumiriam excesso de memória ou CPU, como manipulação de vídeo ou disparos massivos de rede.

**Arquitetura do Worker:**
- Desenvolvido em **Golang** (Go 1.21+).
- **Consumidor RabbitMQ:** Escuta ativamente as filas (ex: `video.process.request.queue`, `email.send.queue`) oriundas do exchange `marketplace.exchange`.
- **Stateless:** Não guarda estado local; arquivos processados são enviados para nuvem (Supabase S3) e os resultados/notificações são devolvidos à fila ou banco de dados.

**Pastas Principais:**
- `cmd/`: Onde fica o ponto de entrada (entry point) da aplicação (`main.go`).
- `internal/`: Todo o código da aplicação e regras de negócio ficam contidos aqui, garantindo o encapsulamento do Go.
