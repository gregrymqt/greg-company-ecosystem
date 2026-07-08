# Build stage
FROM golang:alpine AS builder
WORKDIR /app

# Download dependências
COPY go.mod go.sum ./
RUN go mod download

# Copia o código e compila
COPY . .
RUN go build -o main ./cmd/main.go

# Run stage
FROM alpine:latest
WORKDIR /app

# Instala o FFmpeg para processamento de vídeo HLS
RUN apk add --no-cache ffmpeg

# Copia o binário compilado
COPY --from=builder /app/main .

CMD ["./main"]
