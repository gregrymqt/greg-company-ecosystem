﻿using MeuCrudCsharp.Features.Videos.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Videos.Utils
{
    public static class VideoMapper
    {
        public static VideoDto ToDto(Video video)
        {
            return new VideoDto
            {
                Id = video.PublicId,
                Title = video.Title,
                Description = video.Description,
                StorageIdentifier = video.StorageIdentifier,
                UploadDate = video.UploadDate,
                Duration = video.Duration,
                Status = video.Status.ToString(),
                CourseName = video.Course?.Name ?? string.Empty,
                ThumbnailUrl = video.ThumbnailUrl ?? string.Empty,
            };
        }
    }
}
