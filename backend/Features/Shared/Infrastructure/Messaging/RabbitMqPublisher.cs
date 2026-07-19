using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using MeuCrudCsharp.Features.Shared.Application.Interfaces;

namespace MeuCrudCsharp.Features.Shared.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    private IModel GetChannel()
    {
        if (_channel != null) return _channel;

        lock (_lock)
        {
            if (_channel == null)
            {
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: "marketplace.exchange", type: ExchangeType.Direct, durable: true);
            }
            return _channel;
        }
    }

    public Task PublishAsync(string eventType, string payload)
    {
        try
        {
            var channel = GetChannel();
            var body = Encoding.UTF8.GetBytes(payload);
            
            lock (_lock)
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(
                    exchange: "marketplace.exchange",
                    routingKey: eventType,
                    basicProperties: properties,
                    body: body);
            }
                
            _logger.LogInformation("Published event {EventType} to RabbitMQ.", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to RabbitMQ.", eventType);
            throw;
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _channel?.Dispose();
            _channel = null;
        }
    }
}
