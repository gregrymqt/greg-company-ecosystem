using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services;

public class UpdateAboutAsyncTests
{
    private readonly Mock<IAboutRepository> _repository = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<IFileService> _fileService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AboutService _sut; // SUT = System Under Test (o serviço que testamos)

    public UpdateAboutAsyncTests()
    {
        _sut = new AboutService(_repository.Object, _cache.Object, _fileService.Object, _unitOfWork.Object);
    }

    // --- MÉTODOS AUXILIARES (A Malícia) ---
    private AboutSection CreateFakeEntity(int? fileId = null) => new()
    {
        Id = 1, Title = "Velho", FileId = fileId
    };

    private CreateUpdateAboutSectionDto CreateFakeDto(bool isChunk = false) => new()
    {
        Title = "Novo",
        IsChunk = isChunk,
        FileName = "foto.jpg",
        File = new Mock<IFormFile>().Object
    };

    // --- TESTES ---

    [Theory]
    [InlineData(true)] // Testa como Chunk
    [InlineData(false)] // Testa como Upload Normal
    public async Task UpdateSection_WhenFileExists_ShouldUpdateSuccessfully(bool isChunk)
    {
        // Arrange
        var entity = CreateFakeEntity(fileId: 10);
        var dto = CreateFakeDto(isChunk);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

        // Mock do retorno do arquivo (para ambos os casos de substituição)
        var fileResult = new EntityFile
        {
            Id = 10,
            CaminhoRelativo = "uploads/foto.jpg",
            NomeArquivo = "foto.jpg",
            FeatureCategoria = "AboutTeam",
            TamanhoBytes = 12345,
            ContentType = "image/jpeg"
        };

        _fileService.Setup(f =>
                f.ProcessChunkAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync("temp/path");

        _fileService.Setup(f => f.SubstituirArquivoDoTempAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(fileResult);

        _fileService.Setup(f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(fileResult);

        // Act - IMPORTANTE: Usar await aqui!
        var result = await _sut.UpdateSectionAsync(1, dto);

        // Assert
        Assert.True(result);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateSection_WhenChunkIsIntermediary_ShouldReturnFalseImmediately()
    {
        // Arrange
        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateFakeEntity());
        _fileService.Setup(f =>
                f.ProcessChunkAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((string)null!); // Simula que ainda não é o último chunk

        // Act
        var result = await _sut.UpdateSectionAsync(1, CreateFakeDto(isChunk: true));

        // Assert
        Assert.False(result);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never); // Malícia: garante que não salvou no banco
    }
}