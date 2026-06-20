using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Videos.Application.Interfaces;
using MeuCrudCsharp.Features.Videos.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.Videos.Infrastructure.Persistence.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly IMongoCollection<Video> _videos;
        private readonly IMongoCollection<EntityFile> _files;

        public VideoRepository(IMongoDbContext context)
        {
            _videos = context.GetCollection<Video>("videos");
            _files = context.GetCollection<EntityFile>("files");
        }

        public async Task<Video> GetByIdAsync(string id)
        {
            var video = await _videos.Find(v => v.Id == id).FirstOrDefaultAsync() ??
                   throw new InvalidOperationException("VÃ­deo nÃ£o encontrado.");
            
            if (!string.IsNullOrEmpty(video.FileId))
            {
                video.File = await _files.Find(f => f.Id == video.FileId).FirstOrDefaultAsync();
            }
            return video;
        }

        public async Task<Video> GetByStorageIdentifierAsync(string storageId)
        {
            var video = await _videos.Find(v => v.StorageIdentifier == storageId).FirstOrDefaultAsync() ?? 
                throw new InvalidOperationException("VÃ­deo nÃ£o encontrado.");
            
            if (!string.IsNullOrEmpty(video.FileId))
            {
                video.File = await _files.Find(f => f.Id == video.FileId).FirstOrDefaultAsync();
            }
            return video;
        }

        public async Task AddAsync(Video video)
        {
            await _videos.InsertOneAsync(video);
        }

        public async Task UpdateAsync(Video video)
        {
            await _videos.ReplaceOneAsync(v => v.Id == video.Id, video);
        }

        public async Task UpdateStatusAsync(string videoId, VideoStatus newStatus)
        {
            var update = Builders<Video>.Update.Set(v => v.Status, newStatus);
            await _videos.UpdateOneAsync(v => v.Id == videoId, update);
        }

        public async Task<(List<Video> Items, int TotalCount)> GetAllPaginatedAsync(
            int page,
            int pageSize
        )
        {
            var filter = FilterDefinition<Video>.Empty;
            var totalCount = (int)await _videos.CountDocumentsAsync(filter);

            var items = await _videos.Find(filter)
                .SortByDescending(v => v.UploadDate)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var fileIds = items.Where(v => !string.IsNullOrEmpty(v.FileId)).Select(v => v.FileId).Distinct().ToList();
            if (fileIds.Any())
            {
                var files = await _files.Find(f => fileIds.Contains(f.Id)).ToListAsync();
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.FileId))
                    {
                        item.File = files.FirstOrDefault(f => f.Id == item.FileId);
                    }
                }
            }

            return (items, totalCount);
        }

        public async Task<Video> GetByPublicIdAsync(Guid publicId)
        {
            var video = await _videos.Find(v => v.PublicId == publicId).FirstOrDefaultAsync() ?? 
                throw new InvalidOperationException("VÃ­deo nÃ£o encontrado.");
            
            if (!string.IsNullOrEmpty(video.FileId))
            {
                video.File = await _files.Find(f => f.Id == video.FileId).FirstOrDefaultAsync();
            }
            return video;
        }

        public async Task DeleteAsync(Video video)
        {
            await _videos.DeleteOneAsync(v => v.Id == video.Id);
        }
    }
}



