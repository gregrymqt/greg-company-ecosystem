package messaging

import (
	"encoding/json"
	"fmt"
	"log/slog"

	"go-worker/internal/config"
	"go-worker/internal/queue"

	amqp "github.com/rabbitmq/amqp091-go"
	"github.com/sendgrid/sendgrid-go"
	"github.com/sendgrid/sendgrid-go/helpers/mail"
)

type EmailSendRequest struct {
	To            string `json:"to"`
	Subject       string `json:"subject"`
	HtmlBody      string `json:"htmlBody"`
	PlainTextBody string `json:"plainTextBody"`
}

type EmailConsumer struct {
	conn *queue.RabbitMQConnection
	cfg  *configenv.ConfigEnv
}

func NewEmailConsumer(conn *queue.RabbitMQConnection, cfg *configenv.ConfigEnv) *EmailConsumer {
	return &EmailConsumer{
		conn: conn,
		cfg:  cfg,
	}
}

func (c *EmailConsumer) StartConsuming(exchange, queueName, routingKey string) error {
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

	slog.Info("Waiting for email messages...", "queue", q.Name)

	go func() {
		for d := range msgs {
			var req EmailSendRequest
			if err := json.Unmarshal(d.Body, &req); err != nil {
				slog.Error("Failed to unmarshal email message", "error", err, "body", string(d.Body))
				d.Nack(false, false)
				continue
			}

			slog.Info("Received email send request", "to", req.To, "subject", req.Subject)

			go func(d amqp.Delivery, req EmailSendRequest) {
				client := sendgrid.NewSendClient(c.cfg.SendGridApiKey)

				from := mail.NewEmail(c.cfg.SendGridFromName, c.cfg.SendGridFromEmail)
				to := mail.NewEmail("", req.To)

				message := mail.NewSingleEmail(from, req.Subject, to, req.PlainTextBody, req.HtmlBody)

				response, err := client.Send(message)
				if err != nil {
					slog.Error("Failed to send email via SendGrid", "error", err, "to", req.To)
					d.Nack(false, true)
					return
				}

				if response.StatusCode >= 200 && response.StatusCode < 300 {
					slog.Info("Email sent successfully", "to", req.To, "status", response.StatusCode)
					d.Ack(false)
				} else {
					slog.Error("SendGrid returned non-success status", "status", response.StatusCode, "body", response.Body)

					if response.StatusCode >= 400 && response.StatusCode < 500 {
						if response.StatusCode == 429 {
							d.Nack(false, true)
						} else {
							d.Nack(false, false)
						}
					} else {
						d.Nack(false, true)
					}
				}
			}(d, req)
		}
	}()

	return nil
}
