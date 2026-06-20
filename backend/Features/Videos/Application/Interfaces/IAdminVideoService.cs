using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Application.Interfaces
{
    public interface IAdminVideoService
    {
        Task<VideoDto?> HandleVideoUploadAsync(CreateVideoDto dto);

        Task<Video> UpdateVideoAsync(Guid id, UpdateVideoDto dto);

        Task DeleteVideoAsync(Guid id);

        Task<PaginatedResultDto<VideoDto>> GetAllVideosAsync(int page, int pageSize);
    }
}

