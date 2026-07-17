using System.Text.Json;
using MeuCrudCsharp.Features.Base.Workers;
using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Infrastructure.Hubs;
using MeuCrudCsharp.Features.Videos.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using MeuCrudCsharp.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var video = await videoRepository.GetByPublicIdAsync(Guid.Parse(payload.VideoId));
        if (video == null)
        {
            _logger.LogWarning("Video com ID {VideoId} nao encontrado no banco.", payload.VideoId);
            return;
        }

        if (payload.Success)
        {
            video.Status = VideoStatus.Ready;
            video.Duration = TimeSpan.FromSeconds(payload.DurationInSeconds);
            video.StreamingUrl = payload.ManifestUrl;
            
            if (!string.IsNullOrEmpty(payload.RawVideoUrl))
            {
                video.RawVideoUrl = payload.RawVideoUrl;
            }

            video.LastUpdated = DateTime.UtcNow;
            _logger.LogInformation("Processamento do video {VideoId} finalizado com sucesso. StreamingUrl gerada: {Url}", video.Id, video.StreamingUrl);

            await PropagateVideoReadyToCourseDomain(dbContext, video, payload.DurationInSeconds, cancellationToken);
        }
        else
        {
            video.Status = VideoStatus.Failed;
            video.LastUpdated = DateTime.UtcNow;
            _logger.LogWarning("Falha ao processar o video {VideoId}. Erro: {Error}", video.Id, payload.Error);
        }

        await videoRepository.UpdateAsync(video);

        await cacheService.InvalidateCacheByKeyAsync("videos_cache_version");

        if (video.CourseId.HasValue)
        {
            await cacheService.InvalidateCacheByKeyAsync("courses_cache_version");
        }

        await hubContext.Clients.Group(video.StorageIdentifier).SendAsync("VideoProcessingStatusUpdated", new 
        { 
            videoId = video.Id, 
            status = video.Status.ToString(),
            duration = video.Duration.TotalSeconds
        }, cancellationToken);
    }

    private async Task PropagateVideoReadyToCourseDomain(
        ApplicationDbContext dbContext,
        Video video,
        double durationInSeconds,
        CancellationToken cancellationToken)
    {
        if (!video.CourseId.HasValue)
        {
            _logger.LogDebug("Video {VideoId} nao possui CourseId. Propagacao para Course domain ignorada.", video.Id);
            return;
        }

        var lesson = await dbContext.Lessons
            .FirstOrDefaultAsync(l => l.VideoPublicId == video.Id, cancellationToken);

        if (lesson != null && !lesson.IsVideoAvailable)
        {
            lesson.IsVideoAvailable = true;
            _logger.LogInformation(
                "Lesson {LessonId} marcada como disponivel (video {VideoId} pronto).",
                lesson.Id, video.Id);
        }

        var course = await dbContext.Courses
            .FirstOrDefaultAsync(c => c.Id == video.CourseId.Value, cancellationToken);

        if (course != null)
        {
            var addedMinutes = durationInSeconds / 60.0;
            course.TotalDurationMinutes += addedMinutes;
            _logger.LogInformation(
                "Course {CourseId} atualizado: +{Minutes:F1} min (total: {Total:F1} min).",
                course.Id, addedMinutes, course.TotalDurationMinutes);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public record VideoProcessingCompletedEvent(
        string VideoId,
        string StorageIdentifier,
        double DurationInSeconds,
        bool Success,
        string Error,
        string ManifestUrl,
        string RawVideoUrl
    );
}
