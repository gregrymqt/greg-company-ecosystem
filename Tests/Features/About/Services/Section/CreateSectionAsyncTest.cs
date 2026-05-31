using MeuCrudCsharp.Features.About.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.Section;

public class CreateSectionAsyncTest : AboutServiceTestBase
{
    [Fact]
    public async Task CreateSection_WithoutFile_ShouldSaveEntityAndNotCallFileService()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto();
        dto.File = null;
        dto.FileName = null;

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);

        _repository.Verify(r => r.AddSectionAsync(It.IsAny<AboutSection>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);

        _fileService.Verify(
            f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
            Times.Never
        );
        _fileService.Verify(
            f =>
                f.ProcessChunkAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateSection_WithCompleteFile_ShouldCallSalvarArquivoAsync()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: false);
        var fakeFile = AboutTestFakes.CreateFakeEntityFile();

        _fileService
            .Setup(f => f.SalvarArquivoAsync(dto.File!, It.IsAny<string>()))
            .ReturnsAsync(fakeFile);

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fakeFile.CaminhoRelativo, result.ImageUrl);

        _repository.Verify(r => r.AddSectionAsync(It.IsAny<AboutSection>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _fileService.Verify(f => f.SalvarArquivoAsync(dto.File!, It.IsAny<string>()), Times.Once);
        _fileService.Verify(
            f =>
                f.SalvarArquivoDoTempAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateSection_WithFinalChunk_ShouldCallSalvarArquivoDoTempAsync()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: true);
        var fakeFile = AboutTestFakes.CreateFakeEntityFile();
        const string tempPath = "/temp/path/foto.jpg";

        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(dto.File!, dto.FileName!, It.IsAny<int>(), It.IsAny<int>())
            )
            .ReturnsAsync(tempPath);

        _fileService
            .Setup(f => f.SalvarArquivoDoTempAsync(tempPath, dto.FileName!, It.IsAny<string>()))
            .ReturnsAsync(fakeFile);

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fakeFile.CaminhoRelativo, result.ImageUrl);

        _repository.Verify(r => r.AddSectionAsync(It.IsAny<AboutSection>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _fileService.Verify(
            f => f.ProcessChunkAsync(dto.File!, dto.FileName!, It.IsAny<int>(), It.IsAny<int>()),
            Times.Once
        );
        _fileService.Verify(
            f => f.SalvarArquivoDoTempAsync(tempPath, dto.FileName!, It.IsAny<string>()),
            Times.Once
        );
        _fileService.Verify(
            f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateSection_WhenChunkIsIncomplete_ShouldReturnNullAndNotSave()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: true);

        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(dto.File!, dto.FileName!, It.IsAny<int>(), It.IsAny<int>())
            )
            .ReturnsAsync((string)null!);

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        Assert.Null(result);

        _fileService.Verify(
            f =>
                f.SalvarArquivoDoTempAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
        _repository.Verify(r => r.AddSectionAsync(It.IsAny<AboutSection>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }
}
