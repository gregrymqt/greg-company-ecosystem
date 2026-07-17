using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.Shared.Application.DTOs;
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
