package queue

import (
	"context"
	"encoding/json"
	"fmt"
	"log/slog"
	"video-processor/internal/processor"

	amqp "github.com/rabbitmq/amqp091-go"
)

type Consumer struct {
	conn      *RabbitMQConnection
	processor *processor.FfmpegProcessor
}

type VideoProcessRequest struct {
	VideoId           string `json:"VideoId"`
	StorageIdentifier string `json:"StorageIdentifier"`
	SupabasePath      string `json:"SupabasePath"`
}

// VideoProcessingCompletedEvent mapeia o payload idêntico ao esperado pelo VideoProcessingCompletedConsumer no C#
type VideoProcessingCompletedEvent struct {
	VideoId           string  `json:"VideoId"`
	StorageIdentifier string  `json:"StorageIdentifier"`
	DurationInSeconds float64 `json:"DurationInSeconds"`
	Success           bool    `json:"Success"`
	Error             string  `json:"Error"`
}

func NewConsumer(conn *RabbitMQConnection, proc *processor.FfmpegProcessor) *Consumer {
	return &Consumer{
		conn:      conn,
		processor: proc,
	}
}

func (c *Consumer) StartConsuming(exchange, queueName, routingKey string) error {
	ch, err := c.conn.Conn.Channel()
	if err != nil {
		return fmt.Errorf("failed to open a channel: %w", err)
	}

	err = ch.ExchangeDeclare(exchange, "direct", true, false, false, false, nil)
	if err != nil {
		return fmt.Errorf("failed to declare an exchange: %w", err)
	}

	q, err := ch.QueueDeclare(queueName, true, false, false, false, nil)
	if err != nil {
		return fmt.Errorf("failed to declare a queue: %w", err)
	}

	err = ch.QueueBind(q.Name, routingKey, exchange, false, nil)
	if err != nil {
		return fmt.Errorf("failed to bind a queue: %w", err)
	}

	msgs, err := ch.Consume(q.Name, "", false, false, false, false, nil)
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

			// 1. Executa o processamento e obtém a duração do vídeo em segundos
			durationSeconds, processErr := c.processor.ProcessVideo(req.VideoId, req.StorageIdentifier, req.SupabasePath)

			// 2. Prepara o evento de integração de volta para o .NET 8
			completedEvent := VideoProcessingCompletedEvent{
				VideoId:           req.VideoId,
				StorageIdentifier: req.StorageIdentifier,
				DurationInSeconds: durationSeconds,
				Success:           processErr == nil,
			}
			if processErr != nil {
				slog.Error("Failed to process video", "video_id", req.VideoId, "error", processErr)
				completedEvent.Error = processErr.Error()
			}

			eventBody, err := json.Marshal(completedEvent)
			if err != nil {
				slog.Error("Failed to marshal completion event", "video_id", req.VideoId, "error", err)
				d.Nack(false, false)
				continue
			}

			// 3. Publica o resultado na exchange usando a routing key de retorno
			// O consumidor em C# estará escutando a fila vinculada a esta routing key
			err = ch.PublishWithContext(
				context.Background(),
				exchange,
				"video.process.completed", // Routing Key de Conclusão
				false,
				false,
				amqp.Publishing{
					ContentType:  "application/json",
					DeliveryMode: amqp.Persistent, // Garante resiliência se o broker reiniciar
					Body:         eventBody,
				},
			)

			if err != nil {
				slog.Error("Failed to publish completion event to RabbitMQ", "video_id", req.VideoId, "error", err)
				// Se a notificação falhar, damos Requeue (true) para processar novamente e tentar retransmitir
				d.Nack(false, true)
				continue
			}

			// ACK FINAL: Vídeo processado (com sucesso ou falha controlada), upado e sistema notificado!
			d.Ack(false)
			if processErr == nil {
				slog.Info("Video pipeline completed and core notified successfully", "video_id", req.VideoId)
			} else {
				slog.Info("Video pipeline completed with errors and core notified", "video_id", req.VideoId)
			}
		}
	}()

	return nil
}
