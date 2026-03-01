using System;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Tests.Features.About;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.Section;

public class CreateSectionAsyncTest : AboutServiceTestBase
{
    [Theory]
    [InlineData(false, true)] // Scenario 1: Normal upload (small file)
    [InlineData(true, true)] // Scenario 2: Chunking (last piece, complete file)
    [InlineData(false, false)] // Scenario 3: Create without any file
    public async Task CreateSection_WhenSuccessfulPaths_ShouldSaveEntity(bool isChunk, bool hasFile)
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk);
        dto.File = hasFile ? new Mock<IFormFile>().Object : null;
        dto.FileName = hasFile ? "foto.jpg" : null;

        if (isChunk && hasFile)
        {
            _fileService
                .Setup(f =>
                    f.ProcessChunkAsync(dto.File, dto.FileName, It.IsAny<int>(), It.IsAny<int>())
                )
                .ReturnsAsync("/temp/path/foto.jpg");

            _fileService
                .Setup(f =>
                    f.SalvarArquivoDoTempAsync(
                        "/temp/path/foto.jpg",
                        dto.FileName,
                        It.IsAny<string>()
                    )
                )
                .ReturnsAsync(AboutTestFakes.CreateFakeEntityFile());
        }
        else if (!isChunk && hasFile)
        {
            _fileService
                .Setup(f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()))
                .ReturnsAsync(AboutTestFakes.CreateFakeEntityFile());
        }

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);

        _repository.Verify(r => r.AddSectionAsync(It.IsAny<AboutSection>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);

        if (isChunk && hasFile)
        {
            _fileService.Verify(
                f =>
                    f.ProcessChunkAsync(
                        It.IsAny<IFormFile>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()
                    ),
                Times.Once
            );
            _fileService.Verify(
                f =>
                    f.SalvarArquivoDoTempAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    ),
                Times.Once
            );
        }
        else if (!isChunk && hasFile)
        {
            _fileService.Verify(
                f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Once
            );
        }
        else
        {
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
    }

    [Fact]
    public async Task CreateSection_WhenChunkIsIncomplete_ShouldReturnNullAndNotSave()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: true);

        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(dto.File, dto.FileName, It.IsAny<int>(), It.IsAny<int>())
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
