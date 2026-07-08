package main

import (
	"log/slog"
	"os"
	"os/signal"
	"syscall"
	"video-processor/internal/config"
	"video-processor/internal/processor"
	"video-processor/internal/queue"
	"video-processor/internal/storage" // Novo import para plugar o storage
)

func main() {
	slog.Info("Starting video-processor service...")

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
	consumer := queue.NewConsumer(rmqConn, ffmpegProc)
	
	err = consumer.StartConsuming("marketplace.exchange", "video.process.request.queue", "video.process.request")
	if err != nil {
		slog.Error("Failed to start consuming", "error", err)
		os.Exit(1)
	}

	// Wait for interrupt signal to gracefully shutdown the server
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	slog.Info("Shutting down video-processor service gracefully...")
}