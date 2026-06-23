using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Application.Interfaces;

namespace MeuCrudCsharp.Features.Shared.Infrastructure.Workers;

public class OutboxProcessorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorWorker> _logger;

    public OutboxProcessorWorker(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessorWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na varredura do Outbox.");
            }

            // Aguarda 1 segundo antes da próxima varredura
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessorWorker finalizando.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        // Cria um escopo para resolver serviços scoped (como IMongoDbContext, se for scoped)
        using var scope = _serviceProvider.CreateScope();
        var mongoContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        
        // Aqui tentamos resolver IRabbitMqPublisher. 
        // Se ainda não estiver implementado no projeto, usamos o logger como fallback temporário.
        var rabbitMqPublisher = scope.ServiceProvider.GetService<IRabbitMqPublisher>();

        var outboxCollection = mongoContext.GetCollection<OutboxEvent>("OutboxEvents");

        // Busca apenas os eventos não processados (que não têm erro grave, ou tentar novamente)
        var filter = Builders<OutboxEvent>.Filter.Eq(x => x.Processed, false);
        
        var pendingEvents = await outboxCollection.Find(filter)
                                                  .Limit(100) // Processa em lotes
                                                  .ToListAsync(stoppingToken);

        if (pendingEvents.Count == 0)
            return;

        foreach (var evt in pendingEvents)
        {
            try
            {
                // 1. Publica no RabbitMQ
                if (rabbitMqPublisher != null)
                {
                    await rabbitMqPublisher.PublishAsync(evt.EventType, evt.Payload);
                }
                else
                {
                    _logger.LogWarning("IRabbitMqPublisher não registrado. Evento {EventType} ignorado.", evt.EventType);
                }

                // 2. Marca como processado
                var update = Builders<OutboxEvent>.Update
                                .Set(x => x.Processed, true)
                                .Set(x => x.Error, null);
                
                await outboxCollection.UpdateOneAsync(
                    x => x.Id == evt.Id, 
                    update, 
                    cancellationToken: stoppingToken
                );

                _logger.LogInformation("Evento do Outbox {EventId} publicado com sucesso.", evt.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento {EventId}.", evt.Id);
                
                var errorUpdate = Builders<OutboxEvent>.Update.Set(x => x.Error, ex.Message);
                await outboxCollection.UpdateOneAsync(
                    x => x.Id == evt.Id, 
                    errorUpdate, 
                    cancellationToken: stoppingToken
                );
            }
        }
    }
}
