using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using MeuCrudCsharp.Features.Shared.Application.Interfaces;

namespace MeuCrudCsharp.Features.Shared.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task PublishAsync(string eventType, string payload)
    {
        try
        {
            using var channel = _connection.CreateModel();
            
            // Declare the Direct exchange
            channel.ExchangeDeclare(exchange: "marketplace.exchange", type: ExchangeType.Direct, durable: true);

            var body = Encoding.UTF8.GetBytes(payload);
            
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "marketplace.exchange",
                routingKey: eventType,
                basicProperties: properties,
                body: body);
                
            _logger.LogInformation("Published event {EventType} to RabbitMQ.", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to RabbitMQ.", eventType);
            throw;
        }

        return Task.CompletedTask;
    }
}
