using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.Videos.Application.Interfaces;
using MeuCrudCsharp.Features.Videos.Application.Utils;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using System.Text.Json;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Application.Services;

public class AdminVideoService : IAdminVideoService
{
    private readonly IVideoRepository _videoRepository;
    private readonly IFileService _fileService;
    private readonly ICacheService _cacheService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminVideoService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISupabaseStorageService _supabaseStorageService;
    private readonly ApplicationDbContext _context;

    private const string CatVideo = "Videos";
    private const string CatThumb = "VideoThumbnails";
    private const string VideosCacheVersionKey = "videos_cache_version";

    public AdminVideoService(
        IVideoRepository videoRepository,
        IFileService fileService,
        ICacheService cacheService,
        IWebHostEnvironment env,
        ILogger<AdminVideoService> logger,
        IUnitOfWork unitOfWork,
        ISupabaseStorageService supabaseStorageService,
        ApplicationDbContext context
    )
    {
        _videoRepository = videoRepository;
        _fileService = fileService;
        _cacheService = cacheService;
        _env = env;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _supabaseStorageService = supabaseStorageService;
        _context = context;
    }

    public async Task<VideoDto?> HandleVideoUploadAsync(CreateVideoDto dto)
    {
        var fileId = "";
        var thumbnailUrl = string.Empty;
        var storageIdentifier = Guid.NewGuid().ToString();

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            if (dto is { IsChunk: true, File: not null })
            {
                var fileName = dto.FileName ?? $"{Guid.NewGuid()}.tmp";
                var tempPath = await _fileService.ProcessChunkAsync(dto.File, fileName, dto.ChunkIndex, dto.TotalChunks);
                if (tempPath == null) return null;

                var videoSalvo = await _fileService.SalvarArquivoDoTempAsync(tempPath, fileName, CatVideo);
                var localFilePath = Path.Combine(_env.WebRootPath, videoSalvo.CaminhoRelativo.TrimStart('/'));
                storageIdentifier = await _supabaseStorageService.UploadRawVideoAsync(localFilePath, fileName, "greg-videos-raw");
                await _fileService.DeletarArquivoAsync(videoSalvo.Id.ToString());
                fileId = "";
            }
            else if (dto.File != null)
            {
                var fileName = dto.FileName ?? $"{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";
                var videoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CatVideo);
                var localFilePath = Path.Combine(_env.WebRootPath, videoSalvo.CaminhoRelativo.TrimStart('/'));
                storageIdentifier = await _supabaseStorageService.UploadRawVideoAsync(localFilePath, fileName, "greg-videos-raw");
                await _fileService.DeletarArquivoAsync(videoSalvo.Id.ToString());
                fileId = "";
            }

            if (dto.ThumbnailFile != null)
            {
                var thumbSalva = await _fileService.SalvarArquivoAsync(dto.ThumbnailFile, CatThumb);
                thumbnailUrl = thumbSalva.CaminhoRelativo;
            }

            var entity = new Video
            {
                Title = dto.Title ?? "Sem Titulo",
                Description = dto.Description ?? string.Empty,
                CourseId = dto.CourseId,
                StorageIdentifier = storageIdentifier,
                FileId = Guid.TryParse(fileId, out var parsedFileId) ? parsedFileId : null,
                ThumbnailUrl = thumbnailUrl,
                Status = VideoStatus.Pending,
                UploadDate = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Duration = TimeSpan.Zero,
                RawVideoUrl = $"greg-videos-raw/{storageIdentifier}"
            };

            await _videoRepository.AddAsync(entity);

            var outboxEvent = new OutboxEvent
            {
                EventType = "video.process.request",
                Payload = JsonSerializer.Serialize(new
                {
                    VideoId = entity.Id.ToString(),
                    StorageIdentifier = entity.StorageIdentifier,
                    SupabasePath = $"greg-videos-raw/{entity.StorageIdentifier}"
                })
            };

            await _context.OutboxEvents.AddAsync(outboxEvent);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Video {VideoId} criado com sucesso e evento Outbox inserido. StorageId: {StorageId}", entity.Id, entity.StorageIdentifier);

            return new VideoDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Status = entity.Status.ToString(),
                CourseId = entity.CourseId,
                ThumbnailUrl = entity.ThumbnailUrl ?? string.Empty,
            };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<PaginatedResultDto<VideoDto>> GetAllVideosAsync(int page, int pageSize)
    {
        var cacheVersion = await _cacheService.GetOrCreateAsync(
            VideosCacheVersionKey,
            () => Task.FromResult(Guid.NewGuid().ToString()),
            TimeSpan.FromDays(30)
        );

        var cacheKey = $"AdminVideos_v{cacheVersion}_Page{page}_Size{pageSize}";

        return (await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss. Buscando videos pagina {Page} no banco.", page);
                var result = await _videoRepository.GetAllPaginatedAsync(page, pageSize);

                var dtos = result.Items.Select(v => new VideoDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Description = v.Description,
                    StorageIdentifier = v.StorageIdentifier,
                    ThumbnailUrl = v.ThumbnailUrl ?? string.Empty,
                    Status = v.Status.ToString(),
                    UploadDate = v.UploadDate,
                    CourseId = v.CourseId,
                }).ToList();

                return new PaginatedResultDto<VideoDto>(dtos, result.TotalCount, page, pageSize);
            },
            TimeSpan.FromMinutes(10)
        ))!;
    }

    public async Task<Video> UpdateVideoAsync(Guid id, UpdateVideoDto dto)
    {
        var video = await _videoRepository.GetByPublicIdAsync(id);
        if (video == null) throw new ResourceNotFoundException("Video nao encontrado.");

        if (dto.ThumbnailFile is { Length: > 0 })
        {
            var novaThumb = await _fileService.SalvarArquivoAsync(dto.ThumbnailFile, CatThumb);
            video.ThumbnailUrl = novaThumb.CaminhoRelativo;
        }

        video.Title = dto.Title ?? video.Title;
        video.Description = dto.Description ?? video.Description;

        await _videoRepository.UpdateAsync(video);
        await _unitOfWork.CommitAsync();

        await _cacheService.InvalidateCacheByKeyAsync(VideosCacheVersionKey);
        _logger.LogInformation("Video {VideoId} atualizado com sucesso.", video.Id);
        return video;
    }

    public async Task DeleteVideoAsync(Guid id)
    {
        var video = await _videoRepository.GetByPublicIdAsync(id)
            ?? throw new ResourceNotFoundException("Video nao encontrado");

        var fileId = video.FileId?.ToString();
        var storageIdentifier = video.StorageIdentifier;

        try
        {
            await _videoRepository.DeleteAsync(video);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Video {VideoId} removido do banco com sucesso.", video.Id);

            try
            {
                if (!string.IsNullOrEmpty(fileId) && Guid.TryParse(fileId, out var fid))
                {
                    await _fileService.DeletarArquivoAsync(fid.ToString());
                }
                await _supabaseStorageService.DeleteObjectAsync("greg-videos-raw", storageIdentifier);
                await _supabaseStorageService.DeleteFolderAsync("processed-videos", storageIdentifier);
            }
            catch (Exception fileEx)
            {
                _logger.LogError(fileEx, "Erro ao deletar arquivos fisicos do video {VideoId}.", video.Id);
            }

            await _cacheService.InvalidateCacheByKeyAsync(VideosCacheVersionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar video {VideoId} do banco de dados.", video.Id);
            throw new AppServiceException("Erro ao deletar video.", ex);
        }
    }
}
