using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Videos.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Videos.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly ApiDbContext _context;

        public VideoRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<Video> GetByIdAsync(int id)
        {
            return await _context.Videos.Include(v => v.File).FirstOrDefaultAsync(v => v.Id == id) ??
                   throw new InvalidOperationException("Vídeo não encontrado.");
        }

        public async Task<Video> GetByStorageIdentifierAsync(string storageId)
        {
            return await _context
                .Videos.Include(v => v.File)
                .FirstOrDefaultAsync(v => v.StorageIdentifier == storageId) ?? 
                throw new InvalidOperationException("Vídeo não encontrado.");
        }

        public async Task AddAsync(Video video)
        {
            await _context.Videos.AddAsync(video);
        }

        public async Task UpdateAsync(Video video)
        {
            _context.Videos.Update(video);
            await Task.CompletedTask;
        }

        public async Task UpdateStatusAsync(int videoId, VideoStatus newStatus)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video != null)
            {
                video.Status = newStatus;
            }
        }

        public async Task<(List<Video> Items, int TotalCount)> GetAllPaginatedAsync(
            int page,
            int pageSize
        )
        {
            var query = _context.Videos.AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(v => v.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(v => v.File)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Video> GetByPublicIdAsync(Guid publicId)
        {
            return await _context
                .Videos.Include(v => v.File)
                .FirstOrDefaultAsync(v => v.PublicId == publicId) ?? 
                throw new InvalidOperationException("Vídeo não encontrado.");
        }

        public async Task DeleteAsync(Video video)
        {
            _context.Videos.Remove(video);
            await Task.CompletedTask;
        }
    }
}