using Humanizer;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Files.Services;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System;

namespace Tests.Features.About.Services;

public class CreateTeamMemberAsyncTests // Focado no método de criação de membros
{
    private readonly Mock<IAboutRepository> _aboutRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IFileService> _fileServiceMock; // Mockamos a interface, não a classe real
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly AboutService _sut; // SUT = System Under Test (o que estamos testando)

    private const string ABOUT_CACHE_KEY = "ABOUT_PAGE_CONTENT";

    public CreateTeamMemberAsyncTests()
    {
        _aboutRepositoryMock = new Mock<IAboutRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _fileServiceMock = new Mock<IFileService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Apenas a AboutService é real. Passamos os .Object dos mocks.
        _sut = new AboutService(
            _aboutRepositoryMock.Object,
            _cacheServiceMock.Object,
            _fileServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateTeamMemberAsync_WhenFileIsChunk_ShouldReturnDtoAndSaveCorrectly()
    {
        // 1. Arrange (Preparar)
        var dto = new CreateUpdateTeamMemberDto
        {
            Name = "Lucas Vicente",
            Role = "Developer",
            IsChunk = true,
            File = new Mock<IFormFile>().Object,
            FileName = "foto.jpg",
            ChunkIndex = 0,
            TotalChunks = 1
        };

        // NÃO chamamos o serviço real. APENAS dizemos o que o Mock deve retornar.
        _fileServiceMock.Setup(s => s.ProcessChunkAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync("temp/caminho/foto.jpg");

        _fileServiceMock.Setup(s => s.SalvarArquivoDoTempAsync("temp/caminho/foto.jpg", "foto.jpg", "AboutTeam"))
            .ReturnsAsync(new EntityFile
            {
                Id = 10,
                CaminhoRelativo = "uploads/foto.jpg",
                NomeArquivo = "foto.jpg",
                FeatureCategoria = "AboutTeam",
                TamanhoBytes = 12345,
                ContentType = "image/jpeg"
            });

        // 2. Act (Agir)
        var result = await _sut.CreateTeamMemberAsync(dto);

        // 3. Assert (Verificar)
        Assert.NotNull(result);
        Assert.Equal("Lucas Vicente", result.Name);
        Assert.Equal("uploads/foto.jpg", result.PhotoUrl);

        // Verifica se o banco e o cache foram acionados
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(ABOUT_CACHE_KEY), Times.Once);
    }

    [Fact]
    public async Task CreateTeamMemberAsync_WhenFileIsNotChunk_ShouldReturnDtoAndSaveCorrectly()
    {
        // 1. Arrange (Preparar)

        var dto = new CreateUpdateTeamMemberDto
        {
            Name = "Lucas Vicente",
            Role = "Developer",
            IsChunk = false,
            File = new Mock<IFormFile>().Object,
            FileName = "foto.jpg",
            ChunkIndex = 0,
            TotalChunks = 1
        };

        _fileServiceMock.Setup(s => s.SalvarArquivoAsync(It.IsAny<IFormFile>(),It.IsAny<string>()))
        .ReturnsAsync(new EntityFile
            {
                Id = 11,
                CaminhoRelativo = "uploads/foto.jpg",
                NomeArquivo = "foto.jpg",
                FeatureCategoria = "AboutTeam",
                TamanhoBytes = 54321,
                ContentType = "image/jpeg"
            });

            // 2. Act (Agir)
            var result = await _sut.CreateTeamMemberAsync(dto);

            // 3. Assert (Verificar)
            Assert.NotNull(result);
            Assert.Equal("Lucas Vicente", result.Name);
            Assert.Equal("uploads/foto.jpg", result.PhotoUrl);

            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveAsync(ABOUT_CACHE_KEY), Times.Once);
    }


}