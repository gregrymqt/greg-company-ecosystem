package queue

import (
	"log/slog"
	"time"

	amqp "github.com/rabbitmq/amqp091-go"
)

type RabbitMQConnection struct {
	URL  string
	Conn *amqp.Connection
}

func NewRabbitMQConnection(url string) *RabbitMQConnection {
	return &RabbitMQConnection{URL: url}
}

func (r *RabbitMQConnection) Connect() error {
	var err error
	for i := 0; i < 5; i++ {
		r.Conn, err = amqp.Dial(r.URL)
		if err == nil {
			slog.Info("Successfully connected to RabbitMQ")
			return nil
		}
		slog.Warn("Failed to connect to RabbitMQ, retrying...", "error", err, "attempt", i+1)
		time.Sleep(2 * time.Second)
	}
	return err
}

func (r *RabbitMQConnection) Close() error {
	if r.Conn != nil {
		return r.Conn.Close()
	}
	return nil
}
