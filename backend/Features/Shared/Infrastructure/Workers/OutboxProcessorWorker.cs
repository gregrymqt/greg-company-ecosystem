using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

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

            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessorWorker finalizando.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rabbitMqPublisher = scope.ServiceProvider.GetService<IRabbitMqPublisher>();

        var pendingEvents = await dbContext.OutboxEvents
            .Where(x => !x.Processed)
            .Take(100)
            .ToListAsync(stoppingToken);

        if (pendingEvents.Count == 0)
            return;

        foreach (var evt in pendingEvents)
        {
            try
            {
                if (rabbitMqPublisher != null)
                {
                    await rabbitMqPublisher.PublishAsync(evt.EventType, evt.Payload);
                }
                else
                {
                    _logger.LogWarning("IRabbitMqPublisher nao registrado. Evento {EventType} ignorado.", evt.EventType);
                }

                evt.Processed = true;
                evt.Error = null;
                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Evento do Outbox {EventId} publicado com sucesso.", evt.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento {EventId}.", evt.Id);
                evt.Error = ex.Message;
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
