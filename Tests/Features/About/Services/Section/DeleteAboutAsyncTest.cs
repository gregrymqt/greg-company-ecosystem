using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Models;
using Moq;

namespace Tests.Features.About.Services.Section;

public class DeleteAboutAsyncTest : AboutServiceTestBase
{
    [Fact]
    public async Task DeleteSection_WhenSectionHasFile_ShouldDeleteSectionAndFile()
    {
        // Arrange
        const int fileId = 10;
        var entity = AboutTestFakes.CreateFakeSectionEntity(fileId);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
        _repository
            .Setup(r => r.DeleteSectionAsync(It.IsAny<AboutSection>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteSectionAsync(entity.Id);

        // Assert
        _fileService.Verify(f => f.DeletarArquivoAsync(fileId), Times.Once);
        _repository.Verify(r => r.DeleteSectionAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSection_WhenSectionHasNoFile_ShouldDeleteSectionWithoutCallingFileService()
    {
        // Arrange
        var entity = AboutTestFakes.CreateFakeSectionEntity(fileId: null);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
        _repository
            .Setup(r => r.DeleteSectionAsync(It.IsAny<AboutSection>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteSectionAsync(entity.Id);

        // Assert
        _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        _repository.Verify(r => r.DeleteSectionAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSection_WhenEntityNotFound_ShouldThrowException()
    {
        // Arrange
        int idInexistente = 999;

        _repository
            .Setup(r => r.GetSectionByIdAsync(idInexistente))
            .ReturnsAsync((AboutSection)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.DeleteSectionAsync(idInexistente)
        );

        _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
