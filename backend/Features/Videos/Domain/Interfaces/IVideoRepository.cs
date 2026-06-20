using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using System;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Domain.Interfaces;

public interface IVideoRepository
    {
        Task<Video> GetByIdAsync(string id);
        Task<Video> GetByStorageIdentifierAsync(string storageId);
        Task AddAsync(Video video);
        Task UpdateAsync(Video video);
        Task UpdateStatusAsync(string videoId, VideoStatus newStatus);
        Task<(List<Video> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize);
        Task<Video> GetByPublicIdAsync(Guid publicId);
        Task DeleteAsync(Video video);
    }



