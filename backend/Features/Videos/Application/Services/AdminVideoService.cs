using MeuCrudCsharp.Features.Videos.Domain.Interfaces;
using Hangfire;
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
    private readonly IBackgroundJobClient _jobs;
    private readonly ICacheService _cacheService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminVideoService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISupabaseStorageService _supabaseStorageService;
    private readonly IMongoDbContext _context;

    private const string CatVideo = "Videos";
    private const string CatThumb = "VideoThumbnails";
    private const string VideosCacheVersionKey = "videos_cache_version";

    public AdminVideoService(
        IVideoRepository videoRepository,
        IFileService fileService,
        IBackgroundJobClient jobs,
        ICacheService cacheService,
        IWebHostEnvironment env,
        ILogger<AdminVideoService> logger,
        IUnitOfWork unitOfWork,
        ISupabaseStorageService supabaseStorageService,
        IMongoDbContext context
    )
    {
        _videoRepository = videoRepository;
        _fileService = fileService;
        _jobs = jobs;
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

            var tempPath = await _fileService.ProcessChunkAsync(
                dto.File,
                fileName,
                dto.ChunkIndex,
                dto.TotalChunks
            );

            if (tempPath == null)
                return null;

            var videoSalvo = await _fileService.SalvarArquivoDoTempAsync(
                tempPath,
                fileName,
                CatVideo
            );

            var localFilePath = System.IO.Path.Combine(_env.WebRootPath, videoSalvo.CaminhoRelativo.TrimStart('/'));
            storageIdentifier = await _supabaseStorageService.UploadRawVideoAsync(localFilePath, fileName, "greg-videos-raw");
            await _fileService.DeletarArquivoAsync(videoSalvo.Id);
            fileId = "";
        }
        else if (dto.File != null)
        {
            var fileName = dto.FileName ?? $"{Guid.NewGuid()}{System.IO.Path.GetExtension(dto.File.FileName)}";
            var videoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CatVideo);
            
            var localFilePath = System.IO.Path.Combine(_env.WebRootPath, videoSalvo.CaminhoRelativo.TrimStart('/'));
            storageIdentifier = await _supabaseStorageService.UploadRawVideoAsync(localFilePath, fileName, "greg-videos-raw");
            await _fileService.DeletarArquivoAsync(videoSalvo.Id);
            fileId = "";
        }

        if (dto.ThumbnailFile != null)
        {
            var thumbSalva = await _fileService.SalvarArquivoAsync(dto.ThumbnailFile, CatThumb);
            thumbnailUrl = thumbSalva.CaminhoRelativo;
        }

        var entity = new Video
        {
            Title = dto.Title ?? "Sem Tï¿½tulo",
            Description = dto.Description ?? string.Empty,
            CourseId = dto.CourseId,
            StorageIdentifier = storageIdentifier,
            FileId = fileId,
            ThumbnailUrl = thumbnailUrl,
            Status = VideoStatus.Processing,
            UploadDate = DateTime.UtcNow,
            Duration = TimeSpan.Zero,
        };

        await _videoRepository.AddAsync(entity);

        var outboxEvent = new OutboxEvent
        {
            EventType = "video.process.request",
            Payload = JsonSerializer.Serialize(new 
            { 
                VideoId = entity.Id, 
                StorageIdentifier = entity.StorageIdentifier,
                SupabasePath = entity.StorageIdentifier
            })
        };

        var outboxCollection = _context.GetCollection<OutboxEvent>("OutboxEvents");
        
        if (_unitOfWork.Session != null)
        {
            await outboxCollection.InsertOneAsync(_unitOfWork.Session, outboxEvent);
        }
        else
        {
            await outboxCollection.InsertOneAsync(outboxEvent);
        }

        await _unitOfWork.CommitAsync();

        _logger.LogInformation(
            "Vï¿½deo {VideoId} criado com sucesso e evento Outbox inserido. StorageId: {StorageId}",
            entity.PublicId,
            entity.StorageIdentifier
        );

        return new VideoDto
        {
            Id = entity.PublicId,
            Title = entity.Title,
            Status = entity.Status.ToString(),

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
                _logger.LogInformation("Cache miss. Buscando vï¿½deos pï¿½gina {Page} no banco.", page);

                var result = await _videoRepository.GetAllPaginatedAsync(page, pageSize);

                var dtos = result
                    .Items.Select(v => new VideoDto
                    {
                        Id = v.PublicId,
                        Title = v.Title,
                        Description = v.Description,
                        StorageIdentifier = v.StorageIdentifier,
                        ThumbnailUrl = v.ThumbnailUrl ?? string.Empty,
                        Status = v.Status.ToString(),
                        UploadDate = v.UploadDate,
                    })
                    .ToList();

                return new PaginatedResultDto<VideoDto>(dtos, result.TotalCount, page, pageSize);
            },
            TimeSpan.FromMinutes(10)
        ))!;
    }

    public async Task<Video> UpdateVideoAsync(Guid id, UpdateVideoDto dto)
    {
        var video = await _videoRepository.GetByPublicIdAsync(id);

        if (video == null)
            throw new ResourceNotFoundException("Vï¿½deo nï¿½o encontrado.");

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

        _logger.LogInformation(
            "Vï¿½deo {VideoId} atualizado com sucesso.",
            video.PublicId
        );

        return video;
    }

    public async Task DeleteVideoAsync(Guid id)
    {
        var video = await _videoRepository.GetByPublicIdAsync(id);
        if (video == null)
            throw new ResourceNotFoundException("Vï¿½deo nï¿½o encontrado.");

        var fileId = video.FileId;
        var storageIdentifier = video.StorageIdentifier;

        try
        {
            await _videoRepository.DeleteAsync(video);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation(
                "Vï¿½deo {VideoId} removido do banco com sucesso. Iniciando limpeza de arquivos...",
                video.PublicId
            );

            try
            {
                if (!string.IsNullOrEmpty(fileId))
                {
                    await _fileService.DeletarArquivoAsync(fileId);
                }

                VideoDirectoryHelper.DeleteHlsFolder(_env.WebRootPath, storageIdentifier);

                _logger.LogInformation(
                    "Arquivos fï¿½sicos do vï¿½deo {VideoId} removidos com sucesso.",
                    video.PublicId
                );
            }
            catch (Exception fileEx)
            {
                _logger.LogError(
                    fileEx,
                    "Erro ao deletar arquivos fï¿½sicos do vï¿½deo {VideoId}. Registro jï¿½ foi removido do banco.",
                    video.PublicId
                );
            }

            await _cacheService.InvalidateCacheByKeyAsync(VideosCacheVersionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao deletar vï¿½deo {VideoId} do banco de dados.",
                video.PublicId
            );
            throw new AppServiceException("Erro ao deletar vï¿½deo.", ex);
        }
    }
}







