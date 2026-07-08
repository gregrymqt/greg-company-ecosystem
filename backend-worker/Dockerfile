# ========================================================
# ETAPA 1: Compilação do Binário (Builder)
# ========================================================
FROM golang:1.22-alpine AS builder

# Instala ferramentas essenciais de build (caso precise de CGO ou pacotes específicos)
RUN apk add --no-cache git gcc musl-dev

WORKDIR /app

# Copia os arquivos de dependências primeiro (aproveita o cache de camadas do Docker)
COPY go.mod go.sum ./
RUN go mod download

# Copia o restante do código fonte do worker
COPY . .

# Compila o binário otimizado para produção
# CGO_ENABLED=0 garante um binário puramente estático (sem dependências dinâmicas do SO)
# -ldflags="-s -w" remove informações de debug, reduzindo o tamanho do binário em até 40%
RUN CGO_ENABLED=0 GOOS=linux go build -ldflags="-s -w" -o backend-worker ./cmd/worker/main.go

# ========================================================
# ETAPA 2: Imagem Final de Produção (Runner)
# ========================================================
FROM alpine:3.19 AS runner

# Instala o FFmpeg e certificados de CA (necessários para chamadas HTTPS, como SendGrid)
RUN apk add --no-cache ffmpeg ca-certificates tzdata

WORKDIR /app

# Copia apenas o binário compilado da etapa anterior (Etapa Builder)
COPY --from=builder /app/backend-worker .

# Cria os diretórios necessários para logs e uploads (conforme seus volumes mapeados)
RUN mkdir -p /app/logs /app/wwwroot/uploads

# Expõe a porta 8080 para os Health Checks do Kubernetes (Liveness/Readiness Probes)
EXPOSE 8080

# Comando padrão para iniciar a aplicação
CMD ["./backend-worker"]