using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Tests.Features.About;
using Moq;

namespace Tests.Features.About.Services.Section;

public class DeleteAboutAsyncTest : AboutServiceTestBase
{
    [Theory]
    [InlineData(null)]
    [InlineData(10)]
    public async Task DeleteSection_ShouldExecuteFullFlow(int? fileId)
    {
        // Arrange
        var entity = AboutTestFakes.CreateFakeSectionEntity(fileId);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

        _repository
            .Setup(r => r.DeleteSectionAsync(It.IsAny<AboutSection>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteSectionAsync(entity.Id);

        // Assert
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);

        if (fileId.HasValue)
        {
            _fileService.Verify(f => f.DeletarArquivoAsync(fileId.Value), Times.Once);
        }
        else
        {
            _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        }
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
