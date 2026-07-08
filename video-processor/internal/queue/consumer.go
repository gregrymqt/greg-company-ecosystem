package queue

import (
	"encoding/json"
	"fmt"
	"log/slog"
)

type Consumer struct {
	conn *RabbitMQConnection
}

type VideoProcessRequest struct {
	VideoId           string `json:"VideoId"`
	StorageIdentifier string `json:"StorageIdentifier"`
	SupabasePath      string `json:"SupabasePath"`
}

func NewConsumer(conn *RabbitMQConnection) *Consumer {
	return &Consumer{conn: conn}
}

func (c *Consumer) StartConsuming(exchange, queueName, routingKey string) error {
	ch, err := c.conn.Conn.Channel()
	if err != nil {
		return fmt.Errorf("failed to open a channel: %w", err)
	}

	err = ch.ExchangeDeclare(
		exchange, // name
		"direct", // type
		true,     // durable
		false,    // auto-deleted
		false,    // internal
		false,    // no-wait
		nil,      // arguments
	)
	if err != nil {
		return fmt.Errorf("failed to declare an exchange: %w", err)
	}

	q, err := ch.QueueDeclare(
		queueName, // name
		true,      // durable
		false,     // delete when unused
		false,     // exclusive
		false,     // no-wait
		nil,       // arguments
	)
	if err != nil {
		return fmt.Errorf("failed to declare a queue: %w", err)
	}

	err = ch.QueueBind(
		q.Name,     // queue name
		routingKey, // routing key
		exchange,   // exchange
		false,
		nil)
	if err != nil {
		return fmt.Errorf("failed to bind a queue: %w", err)
	}

	msgs, err := ch.Consume(
		q.Name, // queue
		"",     // consumer
		false,  // auto-ack
		false,  // exclusive
		false,  // no-local
		false,  // no-wait
		nil,    // args
	)
	if err != nil {
		return fmt.Errorf("failed to register a consumer: %w", err)
	}

	slog.Info("Waiting for messages...", "queue", q.Name)

	go func() {
		for d := range msgs {
			var req VideoProcessRequest
			if err := json.Unmarshal(d.Body, &req); err != nil {
				slog.Error("Failed to unmarshal message", "error", err, "body", string(d.Body))
				d.Nack(false, false)
				continue
			}

			slog.Info("Received video process request",
				"video_id", req.VideoId,
				"storage_identifier", req.StorageIdentifier,
				"raw_video_path", req.SupabasePath,
			)

			// Here you would call the FFmpeg processor
			// ...

			d.Ack(false)
		}
	}()

	return nil
}
