# ========================================================
# Estágio 1: Build (Ambiente Go Otimizado)
# ========================================================
FROM golang:1.22-alpine AS builder

WORKDIR /app

# Instala certificados CA caso o worker precise disparar webhooks HTTPS pós-transcodificação
RUN apk add --no-cache ca-certificates

# Cache de camadas: Copia e baixa dependências primeiro
COPY go.mod go.sum ./
RUN go mod download

# Copia o restante do código-fonte
COPY . .

# Compilação Estática:
# - CGO_ENABLED=0: Remove dependências dinâmicas do host
# - -s -w: Remove símbolos de debug para reduzir o tamanho do binário
RUN CGO_ENABLED=0 GOOS=linux go build \
    -ldflags="-s -w" \
    -o /app/transcoder \
    ./cmd/main.go

# ========================================================
# Estágio 2: Runtime (Alpine Seguro com FFmpeg)
# ========================================================
FROM alpine:3.20 AS runner

WORKDIR /app

# Instala o FFmpeg e ferramentas mínimas de runtime de forma limpa
RUN apk add --no-cache \
    ffmpeg \
    ca-certificates

# Criação de um usuário não-root seguro para isolamento do processo
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Cria diretórios para processamento temporário de chunks de vídeo (ex: HLS) 
# e garante permissão de escrita para o usuário não-root
RUN mkdir -p /app/tmp /app/output && \
    chown -R appuser:appgroup /app

# Copia o binário compilado do estágio anterior
COPY --from=builder /app/transcoder .

# Transfere o contexto para o usuário restrito
USER appuser

# Define variáveis de ambiente para a aplicação Go se localizarem
ENV ENV=production \
    TMP_DIR=/app/tmp \
    OUTPUT_DIR=/app/output

ENTRYPOINT ["./transcoder"]