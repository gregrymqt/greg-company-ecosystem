using System.Text.Json;
using MeuCrudCsharp.Features.Base.Workers;
using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Hubs;
using MeuCrudCsharp.Features.Videos.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;

namespace MeuCrudCsharp.Features.Videos.Shared.Infrastructure.Workers;

public class VideoProcessingCompletedConsumer : RabbitMqConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    protected override string QueueName => "video.process.completed.queue";

    public VideoProcessingCompletedConsumer(
        IConnection connection, 
        ILogger<VideoProcessingCompletedConsumer> logger, 
        IServiceProvider serviceProvider) 
        : base(connection, logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recebido evento de conclusao de processamento de video.");
        
        VideoProcessingCompletedEvent? payload;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            payload = JsonSerializer.Deserialize<VideoProcessingCompletedEvent>(message, options);
            
            if (payload == null || !Guid.TryParse(payload.VideoId, out var parsedVideoId) || parsedVideoId == Guid.Empty)
            {
                _logger.LogWarning("Payload invalido ignorado: {Message}", message);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deserializar mensagem do RabbitMQ: {Message}", message);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var videoRepository = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

        var video = await videoRepository.GetByPublicIdAsync(Guid.Parse(payload.VideoId));
        if (video == null)
        {
            _logger.LogWarning("Video com ID {VideoId} nao encontrado no banco.", payload.VideoId);
            return;
        }

        if (payload.Success)
        {
            video.Status = VideoStatus.Available;
            video.Duration = TimeSpan.FromSeconds(payload.DurationInSeconds);
            _logger.LogInformation("Processamento do video {VideoId} finalizado com sucesso.", video.PublicId);
        }
        else
        {
            video.Status = VideoStatus.Error;
            _logger.LogWarning("Falha ao processar o video {VideoId}. Erro: {Error}", video.PublicId, payload.Error);
        }

        await videoRepository.UpdateAsync(video);

        // Invalidate cache
        await cacheService.InvalidateCacheByKeyAsync("videos_cache_version");

        // Notify via SignalR (StorageIdentifier as Group)
        await hubContext.Clients.Group(video.StorageIdentifier).SendAsync("VideoProcessingStatusUpdated", new 
        { 
            videoId = video.PublicId, 
            status = video.Status.ToString(),
            duration = video.Duration.TotalSeconds
        }, cancellationToken);
    }

    public record VideoProcessingCompletedEvent(
        string VideoId,
        string StorageIdentifier,
        double DurationInSeconds,
        bool Success,
        string Error
    );
}
