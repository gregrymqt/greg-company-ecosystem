using System;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.About.Services;

public class AboutService : IAboutService
{
    private readonly IAboutRepository _repository;
    private readonly ICacheService _cache;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;

    private const string ABOUT_CACHE_KEY = "ABOUT_PAGE_CONTENT";
    private const string CAT_SECTION = "AboutSection";
    private const string CAT_TEAM = "AboutTeam";

    public AboutService(
        IAboutRepository repository,
        ICacheService cache,
        IFileService fileService,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _cache = cache;
        _fileService = fileService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AboutPageContentDto> GetAboutPageContentAsync()
    {
        return await _cache.GetOrCreateAsync(
                ABOUT_CACHE_KEY,
                async () =>
                {
                    var sections = await _repository.GetAllSectionsAsync();
                    var members = await _repository.GetAllTeamMembersAsync();

                    return new AboutPageContentDto
                    {
                        Sections =
                        [
                            .. sections.Select(s => new AboutSectionDto
                            {
                                Id = s.Id,
                                Title = s.Title,
                                Description = s.Description,
                                ImageUrl = s.ImageUrl,
                                ImageAlt = s.ImageAlt,
                                ContentType = "section1",
                            }),
                        ],

                        TeamSection = new AboutTeamSectionDto
                        {
                            Title = "Nosso Time",
                            Description = "Conheça os especialistas",
                            ContentType = "section2",
                            Members =
                            [
                                .. members.Select(m => new TeamMemberDto
                                {
                                    Id = m.Id,
                                    Name = m.Name,
                                    Role = m.Role,
                                    PhotoUrl = m.PhotoUrl,
                                    LinkedinUrl = m.LinkedinUrl,
                                    GithubUrl = m.GithubUrl,
                                }),
                            ],
                        },
                    };
                }
            ) ?? new AboutPageContentDto();
    }

    public async Task<AboutSectionDto?> CreateSectionAsync(CreateUpdateAboutSectionDto dto)
    {
        var imageUrl = string.Empty;
        int? fileId = null;

        if (dto is { IsChunk: true, File: not null })
        {
            if (dto.FileName != null)
            {
                var tempPath = await _fileService.ProcessChunkAsync(
                    dto.File,
                    dto.FileName,
                    dto.ChunkIndex,
                    dto.TotalChunks
                );

                if (tempPath == null)
                    return null;

                var arquivoSalvo = await _fileService.SalvarArquivoDoTempAsync(
                    tempPath,
                    dto.FileName,
                    CAT_SECTION
                );
                imageUrl = arquivoSalvo.CaminhoRelativo;
                fileId = arquivoSalvo.Id;
            }
        }
        else if (dto.File != null)
        {
            var arquivoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CAT_SECTION);
            imageUrl = arquivoSalvo.CaminhoRelativo;
            fileId = arquivoSalvo.Id;
        }

        var entity = new AboutSection
        {
            Title = dto.Title,
            Description = dto.Description,
            ImageAlt = dto.ImageAlt,
            ImageUrl = imageUrl,
            FileId = fileId,
            OrderIndex = dto.OrderIndex,
        };

        await _repository.AddSectionAsync(entity);
        await _unitOfWork.CommitAsync();
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);

        return new AboutSectionDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            ImageAlt = entity.ImageAlt,
            ContentType = "section1",
        };
    }

    public async Task<bool> UpdateSectionAsync(int id, CreateUpdateAboutSectionDto dto)
    {
        var entity =
            await _repository.GetSectionByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Seção {id} não encontrada.");

        if (dto is { IsChunk: true, File: not null })
        {
            var tempPath = await _fileService.ProcessChunkAsync(
                dto.File,
                dto.FileName ?? throw new NullReferenceException("FileName is required"),
                dto.ChunkIndex,
                dto.TotalChunks
            );

            if (tempPath == null)
                return false;

            if (entity.FileId.HasValue)
            {
                var arquivoAtualizado = await _fileService.SubstituirArquivoDoTempAsync(
                    entity.FileId.Value,
                    tempPath,
                    dto.FileName
                );
                entity.ImageUrl = arquivoAtualizado.CaminhoRelativo;
                entity.FileId = arquivoAtualizado.Id;
            }
            else
            {
                var arquivoSalvo = await _fileService.SalvarArquivoDoTempAsync(
                    tempPath,
                    dto.FileName,
                    CAT_SECTION
                );
                entity.ImageUrl = arquivoSalvo.CaminhoRelativo;
                entity.FileId = arquivoSalvo.Id;
            }
        }
        else if (dto.File != null)
        {
            if (entity.FileId.HasValue)
            {
                var arquivoAtualizado = await _fileService.SubstituirArquivoAsync(
                    entity.FileId.Value,
                    dto.File
                );
                entity.ImageUrl = arquivoAtualizado.CaminhoRelativo;
                entity.FileId = arquivoAtualizado.Id;
            }
            else
            {
                var arquivoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CAT_SECTION);
                entity.ImageUrl = arquivoSalvo.CaminhoRelativo;
                entity.FileId = arquivoSalvo.Id;
            }
        }

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.ImageAlt = dto.ImageAlt;

        await _repository.UpdateSectionAsync(entity);
        await _unitOfWork.CommitAsync();
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);

        return true;
    }

    public async Task DeleteSectionAsync(int id)
    {
        var entity =
            await _repository.GetSectionByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Seção {id} não encontrada.");

        if (entity.FileId.HasValue)
        {
            await _fileService.DeletarArquivoAsync(entity.FileId.Value);
        }

        await _repository.DeleteSectionAsync(entity);
        await _unitOfWork.CommitAsync();
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);
    }

    public async Task<TeamMemberDto?> CreateTeamMemberAsync(CreateUpdateTeamMemberDto dto)
    {
        var photoUrl = string.Empty;
        int? fileId = null;

        if (dto is { IsChunk: true, File: not null })
        {
            var tempPath = await _fileService.ProcessChunkAsync(
                dto.File,
                dto.FileName!,
                dto.ChunkIndex,
                dto.TotalChunks
            );
            if (tempPath == null)
                return null;

            if (dto.FileName != null)
            {
                var arquivoSalvo = await _fileService.SalvarArquivoDoTempAsync(
                    tempPath,
                    dto.FileName,
                    CAT_TEAM
                );
                photoUrl = arquivoSalvo.CaminhoRelativo;
                fileId = arquivoSalvo.Id;
            }
        }
        else if (dto.File != null)
        {
            var arquivoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CAT_TEAM);
            photoUrl = arquivoSalvo.CaminhoRelativo;
            fileId = arquivoSalvo.Id;
        }

        var entity = new TeamMember
        {
            Name = dto.Name,
            Role = dto.Role,
            LinkedinUrl = dto.LinkedinUrl,
            GithubUrl = dto.GithubUrl,
            PhotoUrl = photoUrl,
            FileId = fileId,
        };

        await _repository.AddTeamMemberAsync(entity);
        await _unitOfWork.CommitAsync();
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);

        return new TeamMemberDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Role = entity.Role,
            PhotoUrl = entity.PhotoUrl,
            LinkedinUrl = entity.LinkedinUrl,
            GithubUrl = entity.GithubUrl,
        };
    }

    public async Task<bool> UpdateTeamMemberAsync(int id, CreateUpdateTeamMemberDto dto)
    {
        var entity =
            await _repository.GetTeamMemberByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Membro {id} não encontrado.");

        entity.Name = dto.Name;
        entity.Role = dto.Role;
        entity.LinkedinUrl = dto.LinkedinUrl;
        entity.GithubUrl = dto.GithubUrl;

        if (dto.File != null)
        {
            if (entity.FileId.HasValue)
            {
                var arquivoAtualizado = await _fileService.SubstituirArquivoAsync(
                    entity.FileId.Value,
                    dto.File
                );
                entity.PhotoUrl = arquivoAtualizado.CaminhoRelativo;
                entity.FileId = arquivoAtualizado.Id;
            }
            else
            {
                var arquivoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CAT_TEAM);
                entity.PhotoUrl = arquivoSalvo.CaminhoRelativo;
                entity.FileId = arquivoSalvo.Id;
            }
        }

        await _repository.UpdateTeamMemberAsync(entity);
        await _unitOfWork.CommitAsync();
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);

        return true;
    }

    public async Task DeleteTeamMemberAsync(int id)
    {
        var entity =
            await _repository.GetTeamMemberByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Membro {id} não encontrado.");

        if (entity.FileId.HasValue)
        {
            await _fileService.DeletarArquivoAsync(entity.FileId.Value);
        }

        await _repository.DeleteTeamMemberAsync(entity);
        await _unitOfWork.CommitAsync();
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);
    }
}
