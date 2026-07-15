using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Application.Interfaces;
using MeuCrudCsharp.Features.Home.Application.DTOs;
using MeuCrudCsharp.Features.Home.Application.Interfaces;
using MeuCrudCsharp.Features.Home.Domain.Entities;
using MeuCrudCsharp.Features.Home.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;

namespace MeuCrudCsharp.Features.Home.Application.Services;

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
                            .OrderBy(h => h.Order)
                            .Select(h => new HeroSlideDto
                            {
                                Id = h.Id,
                                Title = h.Title,
                                Subtitle = h.Subtitle,
                                ImageUrl = h.ImageUrl,
                                CtaText = h.CtaText,
                                CtaLink = h.CtaLink,
                                Audience = h.Audience,
                                Order = h.Order,
                            })
                            .ToList(),

                        Services = services
                            .OrderBy(s => s.Order)
                            .Select(s => new ServiceDto
                            {
                                Id = s.Id,
                                Title = s.Title,
                                Description = s.Description,
                                Icon = s.Icon,
                                CtaText = s.CtaText,
                                CtaLink = s.CtaLink,
                                Audience = s.Audience,
                                Order = s.Order,
                            })
                            .ToList(),
                    };
                }
            ) ?? new HomeContentDto();
    }

    public async Task<HeroSlideDto?> CreateHeroAsync(CreateUpdateHeroDto dto)
    {
        var imageUrl = string.Empty;
        string? fileId = null;

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


        var entity = new HomeHero
        {
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ImageUrl = imageUrl,
            FileId = fileId,
            CtaText = dto.CtaText,
            CtaLink = dto.CtaLink,
            Audience = dto.Audience,
            Order = dto.Order,
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
            CtaText = entity.CtaText,
            CtaLink = entity.CtaLink,
            Audience = entity.Audience,
            Order = entity.Order,
        };
    }

    public async Task<bool> UpdateHeroAsync(string id, CreateUpdateHeroDto dto)
    {
        var entity = await repository.GetHeroByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"Hero com ID {id} nÃƒÂ£o encontrado.");

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

                if (!string.IsNullOrEmpty(entity.FileId))
                {
                    var arquivoAtualizado = await fileService.SubstituirArquivoDoTempAsync(
                        entity.FileId,
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
            if (!string.IsNullOrEmpty(entity.FileId))
            {
                var arquivoAtualizado = await fileService.SubstituirArquivoAsync(
                    entity.FileId,
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
        entity.CtaText = dto.CtaText;
        entity.CtaLink = dto.CtaLink;
        entity.Audience = dto.Audience;
        entity.Order = dto.Order;

        await repository.UpdateHeroAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);

        return true;
    }

    public async Task DeleteHeroAsync(string id)
    {
        var entity = await repository.GetHeroByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"Hero com ID {id} nÃƒÂ£o encontrado.");

        if (!string.IsNullOrEmpty(entity.FileId))
        {
            await fileService.DeletarArquivoAsync(entity.FileId);
        }

        await repository.DeleteHeroAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);
    }

    public async Task<ServiceDto> CreateServiceAsync(CreateUpdateServiceDto dto)
    {
        var entity = new Domain.Entities.HomeServiceEntry
        {
            Title = dto.Title,
            Description = dto.Description,
            Icon = dto.Icon,
            CtaText = dto.CtaText,
            CtaLink = dto.CtaLink,
            Audience = dto.Audience,
            Order = dto.Order,
        };

        await repository.AddServiceAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);

        return new ServiceDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Icon = entity.Icon,
            CtaText = entity.CtaText,
            CtaLink = entity.CtaLink,
            Audience = entity.Audience,
            Order = entity.Order,
        };
    }

    public async Task UpdateServiceAsync(string id, CreateUpdateServiceDto dto)
    {
        var entity = await repository.GetServiceByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"ServiÃƒÂ§o com ID {id} nÃƒÂ£o encontrado.");

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Icon = dto.Icon;
        entity.CtaText = dto.CtaText;
        entity.CtaLink = dto.CtaLink;
        entity.Audience = dto.Audience;
        entity.Order = dto.Order;

        await repository.UpdateServiceAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);
    }

    public async Task DeleteServiceAsync(string id)
    {
        var entity = await repository.GetServiceByIdAsync(id);
        if (entity == null)
            throw new ResourceNotFoundException($"ServiÃƒÂ§o com ID {id} nÃƒÂ£o encontrado.");

        await repository.DeleteServiceAsync(entity);
        await unitOfWork.CommitAsync();
        await cache.RemoveAsync(HOME_CACHE_KEY);
    }
}







