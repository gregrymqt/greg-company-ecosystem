package main

import (
	"log/slog"
	"os"
	"os/signal"
	"syscall"
	"video-processor/internal/config"
	"video-processor/internal/queue"
)

func main() {
	slog.Info("Starting video-processor service...")

	cfg := config.LoadConfig()

	rmqConn := queue.NewRabbitMQConnection(cfg.RabbitMqURL)
	if err := rmqConn.Connect(); err != nil {
		slog.Error("Could not connect to RabbitMQ", "error", err)
		os.Exit(1)
	}
	defer rmqConn.Close()

	consumer := queue.NewConsumer(rmqConn)
	err := consumer.StartConsuming("marketplace.exchange", "video.process.request.queue", "video.process.request")
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
