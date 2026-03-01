using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Tests.Features.About;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.Section;

public class UpdateAboutAsyncTests : AboutServiceTestBase
{
    // --- TESTES ---

    [Theory]
    [InlineData(true)] // Scenario 1: Updating with a chunked file (simulating an upload in progress)
    [InlineData(false)] // Scenario 2: Updating with a complete file (simulating a regular update)
    public async Task UpdateSection_WhenFileExists_ShouldUpdateSuccessfully(bool isChunk)
    {
        // Arrange
        var entity = AboutTestFakes.CreateFakeSectionEntity(fileId: 10);
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync("temp/path");

        _fileService
            .Setup(f =>
                f.SubstituirArquivoDoTempAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(AboutTestFakes.CreateFakeEntityFile());

        _fileService
            .Setup(f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(AboutTestFakes.CreateFakeEntityFile());

        // Act
        var result = await _sut.UpdateSectionAsync(1, dto);

        // Assert
        Assert.True(result);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSection_WhenChunkIsIntermediary_ShouldReturnFalseImmediately()
    {
        // Arrange
        _repository
            .Setup(r => r.GetSectionByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(AboutTestFakes.CreateFakeSectionEntity());
        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync((string)null!); // Simulate an incomplete upload

        // Act
        var result = await _sut.UpdateSectionAsync(
            1,
            AboutTestFakes.CreateFakeAboutSectionDto(isChunk: true)
        );

        // Assert
        Assert.False(result);

        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSection_WhenSectionDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        int idInexistente = 999;
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: false);

        _repository
            .Setup(r => r.GetSectionByIdAsync(idInexistente))
            .ReturnsAsync((AboutSection)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.UpdateSectionAsync(idInexistente, dto)
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
        _fileService.Verify(
            f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
            Times.Never
        );
        _repository.Verify(r => r.UpdateSectionAsync(It.IsAny<AboutSection>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSection_WhenFileServiceThrowsException_ShouldThrowAndAbortTransaction()
    {
        // Arrange
        int idValido = 1;
        var entity = AboutTestFakes.CreateFakeSectionEntity(fileId: 10);
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: false);

        _repository.Setup(r => r.GetSectionByIdAsync(idValido)).ReturnsAsync(entity);

        _fileService
            .Setup(f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()))
            .ThrowsAsync(new Exception("Falha simulada no disco ao salvar o arquivo."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _sut.UpdateSectionAsync(idValido, dto)
        );

        Assert.Equal("Falha simulada no disco ao salvar o arquivo.", exception.Message);

        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
