using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Videos.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Videos.Interfaces
{
    public interface IAdminVideoService
    {
        Task<VideoDto?> HandleVideoUploadAsync(CreateVideoDto dto);

        Task<Video> UpdateVideoAsync(Guid id, UpdateVideoDto dto);

        Task DeleteVideoAsync(Guid id);

        Task<PaginatedResultDto<VideoDto>> GetAllVideosAsync(int page, int pageSize);
    }
}
