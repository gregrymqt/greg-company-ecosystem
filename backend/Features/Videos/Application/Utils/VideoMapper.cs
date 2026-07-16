using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Application.Utils
{
    public static class VideoMapper
    {
        public static VideoDto ToDto(Video video)
        {
            return new VideoDto
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                StorageIdentifier = video.StorageIdentifier,
                UploadDate = video.UploadDate,
                Duration = video.Duration,
                Status = video.Status.ToString(),
                RawVideoUrl = video.RawVideoUrl,
                StreamingUrl = video.StreamingUrl,
                CourseName = video.Course?.Name ?? string.Empty,
                ThumbnailUrl = video.ThumbnailUrl ?? string.Empty,
            };
        }
    }
}
