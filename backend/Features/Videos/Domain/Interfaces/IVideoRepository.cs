using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Domain.Interfaces;

public interface IVideoRepository
{
    Task<Video> GetByIdAsync(Guid id);
    Task<Video> GetByStorageIdentifierAsync(string storageId);
    Task AddAsync(Video video);
    Task UpdateAsync(Video video);
    Task UpdateStatusAsync(Guid videoId, VideoStatus newStatus);
    Task<(List<Video> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize, VideoStatus? status = null);
    Task<Video> GetByPublicIdAsync(Guid publicId);
    Task DeleteAsync(Video video);
}
