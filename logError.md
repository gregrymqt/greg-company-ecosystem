public async Task<bool> UpdateTeamMemberAsync(int id, CreateUpdateTeamMemberDto dto)
    {
        var entity =
            await _repository.GetTeamMemberByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Membro {id} não encontrado.");

        entity.Name = dto.Name;
        entity.Role = dto.Role;
        entity.LinkedinUrl = dto.LinkedinUrl;
        entity.GithubUrl = dto.GithubUrl;

        // UPDATE: Substituição inteligente
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
        await _unitOfWork.CommitAsync(); // Persiste no banco
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);

        return true;
    }
---
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;

namespace Tests.Features.About.Services;

using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Http;
using Moq;

public abstract class AboutServiceTestBase
{
    // Usamos 'protected' para as classes que herdarem conseguirem usar
    protected readonly Mock<IAboutRepository> _repository;
    protected readonly Mock<IFileService> _fileService;
    protected readonly Mock<IUnitOfWork> _unitOfWork;
    protected readonly Mock<ICacheService> _cache;

    // O seu Service pronto para ser testado
    protected readonly AboutService _sut;

    protected AboutServiceTestBase()
    {
        // Inicializa todos os mocks do zero para cada teste
        _repository = new Mock<IAboutRepository>();
        _fileService = new Mock<IFileService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cache = new Mock<ICacheService>();

        // Instancia o serviço injetando os objetos falsos
        _sut = new AboutService(
            _repository.Object,
            _cache.Object,
            _fileService.Object,
            _unitOfWork.Object
        );
    }

    // Na sua classe base abstrata:
    protected AboutSection CreateFakeSectionEntity(int? fileId = null) =>
        new() { Id = 1, Title = "Velho", FileId = fileId };

    protected CreateUpdateAboutSectionDto CreateFakeAboutSectionDto(bool isChunk = false) =>
        new() { Title = "Novo", IsChunk = isChunk, FileName = "foto.jpg", File = new Mock<IFormFile>().Object };

    protected TeamMember CreateFakeTeamMemberEntity(int? fileId = null) =>
        new() { Id = 1, Name = "Lucas Vicente", Role = "Developer", FileId = fileId };

    protected CreateUpdateTeamMemberDto CreateFakeTeamMemberDto(bool isChunk = false) =>
    new()
    {
        Name = "Lucas Vicente",
        Role = "Developer",
        IsChunk = isChunk,
        File = new Mock<IFormFile>().Object,
        FileName = "foto.jpg",
        ChunkIndex = 0,
        TotalChunks = 1,
    };

    protected EntityFile CreateFakeEntityFile() =>
        new EntityFile
        {
            Id = 10,
            CaminhoRelativo = "uploads/foto.jpg",
            NomeArquivo = "foto.jpg",
            FeatureCategoria = "About",
            TamanhoBytes = 12345,
            ContentType = "image/jpeg",
        };
}