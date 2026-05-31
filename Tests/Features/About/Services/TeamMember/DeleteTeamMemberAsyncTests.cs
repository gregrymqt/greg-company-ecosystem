using MeuCrudCsharp.Features.Exceptions;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class DeleteTeamMemberAsyncTests : AboutServiceTestBase
{
    [Fact]
    public async Task DeleteTeamMember_WhenMemberHasFile_ShouldDeleteMemberAndFile()
    {
        // Arrange
        const int idMembro = 1;
        const int fileId = 10;
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: fileId);

        _repository.Setup(r => r.GetTeamMemberByIdAsync(idMembro)).ReturnsAsync(entity);

        // Act
        await _sut.DeleteTeamMemberAsync(idMembro);

        // Assert
        _fileService.Verify(f => f.DeletarArquivoAsync(fileId), Times.Once);
        _repository.Verify(r => r.DeleteTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTeamMember_WhenMemberHasNoFile_ShouldDeleteMemberWithoutCallingFileService()
    {
        // Arrange
        const int idMembro = 1;
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: null);

        _repository.Setup(r => r.GetTeamMemberByIdAsync(idMembro)).ReturnsAsync(entity);

        // Act
        await _sut.DeleteTeamMemberAsync(idMembro);

        // Assert
        _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        _repository.Verify(r => r.DeleteTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTeamMember_WhenMemberNotFound_ShouldThrowAndAbort()
    {
        // Arrange
        const int nonExistentId = 999;

        _repository
            .Setup(r => r.GetTeamMemberByIdAsync(nonExistentId))
            .ReturnsAsync((MeuCrudCsharp.Features.About.Domain.Entities.TeamMember)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.DeleteTeamMemberAsync(nonExistentId)
        );

        Assert.Equal($"Membro {nonExistentId} não encontrado.", exception.Message);

        _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        _repository.Verify(
            r => r.DeleteTeamMemberAsync(It.IsAny<MeuCrudCsharp.Features.About.Domain.Entities.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
