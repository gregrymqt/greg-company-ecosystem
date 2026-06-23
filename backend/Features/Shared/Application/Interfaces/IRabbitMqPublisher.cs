namespace MeuCrudCsharp.Features.Shared.Application.Interfaces;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string eventType, string payload);
}
