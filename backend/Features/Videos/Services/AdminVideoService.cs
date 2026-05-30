﻿using Hangfire;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Features.Videos.DTOs;
using MeuCrudCsharp.Features.Videos.Interfaces;
using MeuCrudCsharp.Features.Videos.Utils;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Videos.Services;

public class AdminVideoService : IAdminVideoService
{
    private readonly IVideoRepository _videoRepository;
    private readonly IFileService _fileService;
    private readonly IBackgroundJobClient _jobs;
    private readonly ICacheService _cacheService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminVideoService> _logger;
    private readonly IUnitOfWork _unitOfWork;

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
        IUnitOfWork unitOfWork
    )
    {
        _videoRepository = videoRepository;
        _fileService = fileService;
        _jobs = jobs;
        _cacheService = cacheService;
        _env = env;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<VideoDto?> HandleVideoUploadAsync(CreateVideoDto dto)
    {
        var fileId = 0;
        var thumbnailUrl = string.Empty;
        var storageIdentifier = Guid.NewGuid().ToString();

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

            fileId = videoSalvo.Id;
        }
        else if (dto.File != null)
        {
            var videoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CatVideo);
            fileId = videoSalvo.Id;
        }

        if (dto.ThumbnailFile != null)
        {
            var thumbSalva = await _fileService.SalvarArquivoAsync(dto.ThumbnailFile, CatThumb);
            thumbnailUrl = thumbSalva.CaminhoRelativo;
        }

        var entity = new Video
        {
            Title = dto.Title ?? "Sem Título",
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

        await _unitOfWork.CommitAsync();

        _logger.LogInformation(
            "Vídeo {VideoId} criado com sucesso. StorageId: {StorageId}",
            entity.PublicId,
            entity.StorageIdentifier
        );

        _jobs.Enqueue<IVideoProcessingService>(
            service => service.ProcessVideoToHlsAsync(entity.Id, entity.FileId)
        );

        _logger.LogInformation(
            "Job de processamento HLS agendado para o vídeo {VideoId}",
            entity.PublicId
        );

        return new VideoDto
        {
            Id = entity.PublicId,
            Title = entity.Title,
            Status = entity.Status.ToString(),

            ThumbnailUrl = entity.ThumbnailUrl ?? string.Empty,
        };
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
                _logger.LogInformation("Cache miss. Buscando vídeos página {Page} no banco.", page);

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
            throw new ResourceNotFoundException("Vídeo não encontrado.");

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
            "Vídeo {VideoId} atualizado com sucesso.",
            video.PublicId
        );

        return video;
    }

    public async Task DeleteVideoAsync(Guid id)
    {
        var video = await _videoRepository.GetByPublicIdAsync(id);
        if (video == null)
            throw new ResourceNotFoundException("Vídeo não encontrado.");

        var fileId = video.FileId;
        var storageIdentifier = video.StorageIdentifier;

        try
        {
            await _videoRepository.DeleteAsync(video);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation(
                "Vídeo {VideoId} removido do banco com sucesso. Iniciando limpeza de arquivos...",
                video.PublicId
            );

            try
            {
                if (fileId > 0)
                {
                    await _fileService.DeletarArquivoAsync(fileId);
                }

                VideoDirectoryHelper.DeleteHlsFolder(_env.WebRootPath, storageIdentifier);

                _logger.LogInformation(
                    "Arquivos físicos do vídeo {VideoId} removidos com sucesso.",
                    video.PublicId
                );
            }
            catch (Exception fileEx)
            {
                _logger.LogError(
                    fileEx,
                    "Erro ao deletar arquivos físicos do vídeo {VideoId}. Registro já foi removido do banco.",
                    video.PublicId
                );
            }

            await _cacheService.InvalidateCacheByKeyAsync(VideosCacheVersionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao deletar vídeo {VideoId} do banco de dados.",
                video.PublicId
            );
            throw new AppServiceException("Erro ao deletar vídeo.", ex);
        }
    }
}
