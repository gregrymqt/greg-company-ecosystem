using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Home.DTOs;
using MeuCrudCsharp.Features.Home.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;

namespace MeuCrudCsharp.Features.Home.Services;

public class HomeService(
    IHomeRepository repository,
    ICacheService cache,
    IFileService fileService,
    IUnitOfWork unitOfWork)
    : IHomeService
{
    private const string HOME_CACHE_KEY = "HOME_PAGE_CONTENT";
    private const string FEATURE_CATEGORY = "HomeHero";

    public async Task<HomeContentDto> GetHomeContentAsync()
    {
        return await cache.GetOrCreateAsync(
                HOME_CACHE_KEY,
                async () =>
                {
                    var heroes = await repository.GetAllHeroesAsync();
                    var services = await repository.GetAllServicesAsync();

                    return new HomeContentDto
                    {
                        Hero = heroes
                            .Select(h => new HeroSlideDto
                            {
                                Id = h.Id,
                                Title = h.Title,
                                Subtitle = h.Subtitle,
                                ImageUrl = h.ImageUrl,
                                ActionText = h.ActionText,
                                ActionUrl = h.ActionUrl,
                            })
                            .ToList(),

                        Services = services
                            .Select(s => new ServiceDto
                            {
                                Id = s.Id,
                                Title = s.Title,
                                Description = s.Description,
                                IconClass = s.IconClass,
                                ActionText = s.ActionText,
                                ActionUrl = s.ActionUrl,
                            })
                            .ToList(),
                    };
                }
            ) ?? new HomeContentDto();
    }

    public async Task<HeroSlideDto?> CreateHeroAsync(CreateUpdateHeroDto dto)
    {
        var imageUrl = string.Empty;
        int? fileId = null;

        if (dto is { IsChunk: true, File: not null })
        {
            if (dto.FileName != null)
            {
                var tempPath = await fileService.ProcessChunkAsync(
                    dto.File,
                    dto.FileName,
                    dto.ChunkIndex,
                    dto.TotalChunks
                );

                if (tempPath == null)
                    return null;

                var arquivoSalvo = await fileService.SalvarArquivoDoTempAsync(
                    tempPath,
                    dto.FileName,
                    FEATURE_CATEGORY
                );
                imageUrl = arquivoSalvo.CaminhoRelativo;
                fileId = arquivoSalvo.Id;
            }
        }
        else if (dto.File != null)
        {
            var arquivoSalvo = await fileService.SalvarArquivoAsync(dto.File, FEATURE_CATEGORY);
            imageUrl = arquivoSalvo.CaminhoRelativo;
            fileId = arquivoSalvo.Id;
        }


        var entity = new Models.HomeHero
        {
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ImageUrl = imageUrl,
            FileId = fileId,
            ActionText = dto.ActionText,
            ActionUrl = dto.ActionUrl,
        };

        await repository.AddHeroAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);

        return new HeroSlideDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Subtitle = entity.Subtitle,
            ImageUrl = entity.ImageUrl,
            ActionText = entity.ActionText,
            ActionUrl = entity.ActionUrl,
        };
    }

    public async Task<bool> UpdateHeroAsync(int id, CreateUpdateHeroDto dto)
    {
        var entity = await repository.GetHeroByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"Hero com ID {id} não encontrado.");

        if (dto is { IsChunk: true, File: not null })
        {
            if (dto.FileName != null)
            {
                var tempPath = await fileService.ProcessChunkAsync(
                    dto.File,
                    dto.FileName,
                    dto.ChunkIndex,
                    dto.TotalChunks
                );

                if (tempPath == null)
                    return false;

                if (entity.FileId.HasValue)
                {
                    var arquivoAtualizado = await fileService.SubstituirArquivoDoTempAsync(
                        entity.FileId.Value,
                        tempPath,
                        dto.FileName
                    );
                    entity.ImageUrl = arquivoAtualizado.CaminhoRelativo;
                    entity.FileId = arquivoAtualizado.Id;
                }
                else
                {
                    var arquivoSalvo = await fileService.SalvarArquivoDoTempAsync(
                        tempPath,
                        dto.FileName,
                        FEATURE_CATEGORY
                    );
                    entity.ImageUrl = arquivoSalvo.CaminhoRelativo;
                    entity.FileId = arquivoSalvo.Id;
                }
            }
        }
        else if (dto.File != null)
        {
            if (entity.FileId.HasValue)
            {
                var arquivoAtualizado = await fileService.SubstituirArquivoAsync(
                    entity.FileId.Value,
                    dto.File
                );
                entity.ImageUrl = arquivoAtualizado.CaminhoRelativo;
                entity.FileId = arquivoAtualizado.Id;
            }
            else
            {
                var arquivoSalvo = await fileService.SalvarArquivoAsync(
                    dto.File,
                    FEATURE_CATEGORY
                );
                entity.ImageUrl = arquivoSalvo.CaminhoRelativo;
                entity.FileId = arquivoSalvo.Id;
            }
        }

        entity.Title = dto.Title;
        entity.Subtitle = dto.Subtitle;
        entity.ActionText = dto.ActionText;
        entity.ActionUrl = dto.ActionUrl;

        await repository.UpdateHeroAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);

        return true;
    }

    public async Task DeleteHeroAsync(int id)
    {
        var entity = await repository.GetHeroByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"Hero com ID {id} não encontrado.");

        if (entity.FileId.HasValue)
        {
            await fileService.DeletarArquivoAsync(entity.FileId.Value);
        }

        await repository.DeleteHeroAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);
    }

    public async Task<ServiceDto> CreateServiceAsync(CreateUpdateServiceDto dto)
    {
        var entity = new Models.HomeService
        {
            Title = dto.Title,
            Description = dto.Description,
            IconClass = dto.IconClass,
            ActionText = dto.ActionText,
            ActionUrl = dto.ActionUrl,
        };

        await repository.AddServiceAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);

        return new ServiceDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            IconClass = entity.IconClass,
            ActionText = entity.ActionText,
            ActionUrl = entity.ActionUrl,
        };
    }

    public async Task UpdateServiceAsync(int id, CreateUpdateServiceDto dto)
    {
        var entity = await repository.GetServiceByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"Serviço com ID {id} não encontrado.");

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.IconClass = dto.IconClass;
        entity.ActionText = dto.ActionText;
        entity.ActionUrl = dto.ActionUrl;

        await repository.UpdateServiceAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);
    }

    public async Task DeleteServiceAsync(int id)
    {
        var entity = await repository.GetServiceByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"Serviço com ID {id} não encontrado.");

        await repository.DeleteServiceAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);
    }
}
