package main

import (
	"log/slog"
	"os"
	"os/signal"
	"syscall"
	"go-worker/internal/config"
	"go-worker/internal/messaging"
	"go-worker/internal/processor"
	"go-worker/internal/queue"
	"go-worker/internal/storage" // Novo import para plugar o storage
)

func main() {
	slog.Info("Starting go-worker service...")

	cfg := configenv.LoadConfig()

	// 1. Inicializa o cliente S3 do Supabase
	supabaseStorage, err := storage.NewSupabaseStorage(cfg)
	if err != nil {
		slog.Error("Failed to initialize Supabase Storage client", "error", err)
		os.Exit(1)
	}

	// 2. Conecta ao barramento do RabbitMQ
	rmqConn := queue.NewRabbitMQConnection(cfg.RabbitMqURL)
	if err := rmqConn.Connect(); err != nil {
		slog.Error("Could not connect to RabbitMQ", "error", err)
		os.Exit(1)
	}
	defer rmqConn.Close()

	// 3. Injeta o Supabase Storage dentro do processador do FFmpeg
	ffmpegProc := processor.NewFfmpegProcessor(supabaseStorage)

	// 4. Injeta o processador completo no Consumidor de Mensagens
	videoConsumer := queue.NewConsumer(rmqConn, ffmpegProc)
	err = videoConsumer.StartConsuming("marketplace.exchange", "video.process.request.queue", "video.process.request")
	if err != nil {
		slog.Error("Failed to start consuming videos", "error", err)
		os.Exit(1)
	}

	// 5. Inicializa o Consumidor de E-mails
	emailConsumer := messaging.NewEmailConsumer(rmqConn, cfg)
	err = emailConsumer.StartConsuming("marketplace.exchange", "email.send.queue", "email.send.requested")
	if err != nil {
		slog.Error("Failed to start consuming emails", "error", err)
		os.Exit(1)
	}

	// Wait for interrupt signal to gracefully shutdown the server
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	slog.Info("Shutting down go-worker service gracefully...")
}