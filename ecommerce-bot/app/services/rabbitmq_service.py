import os
import pika
import json
import logging
from app.models.messages import ImportCompletedMessage

logger = logging.getLogger(__name__)

class RabbitMQService:
    def __init__(self):
        self.host = os.getenv("RABBITMQ_HOST", "localhost")
        self.user = os.getenv("RABBITMQ_USER", "guest")
        self.password = os.getenv("RABBITMQ_PASS", "guest")
        self.port = int(os.getenv("RABBITMQ_PORT", "5672"))
        
        self.connection = None
        self.channel = None

    def connect(self):
        try:
            credentials = pika.PlainCredentials(self.user, self.password)
            parameters = pika.ConnectionParameters(
                host=self.host,
                port=self.port,
                credentials=credentials
            )
            self.connection = pika.BlockingConnection(parameters)
            self.channel = self.connection.channel()
            logger.info(f"Successfully connected to RabbitMQ at {self.host}:{self.port}")
        except Exception as e:
            logger.error(f"Failed to connect to RabbitMQ at {self.host}:{self.port}: {str(e)}")
            self.connection = None
            self.channel = None

    def publish_completed_event(self, payload: ImportCompletedMessage):
        if not self.connection or self.connection.is_closed:
            self.connect()
            
        if not self.channel:
            logger.error("Cannot publish event. RabbitMQ channel is not initialized.")
            return

        try:
            # Declare the direct exchange to ensure it exists
            exchange_name = "marketplace.exchange"
            self.channel.exchange_declare(exchange=exchange_name, exchange_type="direct", durable=True)
            
            # Publish message
            routing_key = "product.import.completed"
            message_body = payload.model_dump_json(by_alias=True)
            
            self.channel.basic_publish(
                exchange=exchange_name,
                routing_key=routing_key,
                body=message_body,
                properties=pika.BasicProperties(
                    delivery_mode=pika.DeliveryMode.Persistent
                )
            )
            logger.info(f"Successfully published event to {exchange_name} with routing key '{routing_key}'")
        except Exception as e:
            logger.error(f"Failed to publish event to RabbitMQ: {str(e)}")
            
    def close(self):
        if self.connection and not self.connection.is_closed:
            self.connection.close()
            logger.info("RabbitMQ connection closed.")

# Singleton instance for easy import if needed
rabbitmq_service = RabbitMQService()
