using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Application.Utils
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

