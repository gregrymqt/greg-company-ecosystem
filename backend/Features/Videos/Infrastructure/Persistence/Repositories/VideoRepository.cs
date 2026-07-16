using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Videos.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Videos.Infrastructure.Persistence.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationDbContext _context;

        public VideoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Video> GetByIdAsync(Guid id)
        {
            var video = await _context.Videos
                .Include(v => v.File)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
                throw new InvalidOperationException("Video nao encontrado.");

            return video;
        }

        public async Task<Video> GetByStorageIdentifierAsync(string storageId)
        {
            var video = await _context.Videos
                .Include(v => v.File)
                .FirstOrDefaultAsync(v => v.StorageIdentifier == storageId);

            if (video == null)
                throw new InvalidOperationException("Video nao encontrado.");

            return video;
        }

        public async Task AddAsync(Video video)
        {
            await _context.Videos.AddAsync(video);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Video video)
        {
            _context.Videos.Update(video);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(Guid videoId, VideoStatus newStatus)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video != null)
            {
                video.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Video> Items, int TotalCount)> GetAllPaginatedAsync(
            int page,
            int pageSize,
            VideoStatus? status = null
        )
        {
            var query = _context.Videos.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(v => v.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var fileIds = items.Where(v => v.FileId.HasValue).Select(v => v.FileId!.Value).Distinct().ToList();
            if (fileIds.Any())
            {
                var files = await _context.EntityFiles.Where(f => fileIds.Contains(f.Id)).ToListAsync();
                foreach (var item in items)
                {
                    if (item.FileId.HasValue)
                    {
                        item.File = files.FirstOrDefault(f => f.Id == item.FileId.Value);
                    }
                }
            }

            return (items, totalCount);
        }

        public async Task<Video> GetByPublicIdAsync(Guid publicId)
        {
            var video = await _context.Videos
                .Include(v => v.File)
                .FirstOrDefaultAsync(v => v.Id == publicId);

            if (video == null)
                throw new InvalidOperationException("Video nao encontrado.");

            return video;
        }

        public async Task DeleteAsync(Video video)
        {
            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();
        }
    }
}
