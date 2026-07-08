using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MeuCrudCsharp.Features.Base.Workers;

public abstract class RabbitMqConsumerBase : BackgroundService
{
    private readonly IConnection _connection;
    protected readonly ILogger _logger;
    private IModel? _channel;

    protected abstract string QueueName { get; }

    protected RabbitMqConsumerBase(IConnection connection, ILogger logger)
    {
        _connection = connection;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel is null. Consumer for {QueueName} will not start.", QueueName);
            return Task.CompletedTask;
        }

        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                await ProcessMessageAsync(message, stoppingToken);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event from queue {QueueName}.", QueueName);
                _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        
        return Task.CompletedTask;
    }

    protected abstract Task ProcessMessageAsync(string message, CancellationToken cancellationToken);

    public override void Dispose()
    {
        _channel?.Close();
        base.Dispose();
    }
}
