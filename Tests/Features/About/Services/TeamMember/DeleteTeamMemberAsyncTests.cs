using System;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Tests.Features.About;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class DeleteTeamMemberAsyncTests : AboutServiceTestBase
{
    [Theory]
    [InlineData(10)] // Scenario 1: Member has an associated file (FileId = 10)
    [InlineData(null)] // Scenario 2: Member has no associated file (FileId = null)
    public async Task DeleteTeamMember_WhenSuccessful_ShouldDeleteMemberAndFileIfExists(int? fileId)
    {
        // Arrange
        int idMembro = 1;

        // Criamos a entidade falsa repassando o fileId (que pode ser 10 ou null, dependendo do InlineData)
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: fileId);

        _repository.Setup(r => r.GetTeamMemberByIdAsync(idMembro)).ReturnsAsync(entity);

        // Act
        await _sut.DeleteTeamMemberAsync(idMembro);

        // Assert
        if (fileId.HasValue)
        {
            _fileService.Verify(f => f.DeletarArquivoAsync(fileId.Value), Times.Once);
        }
        else
        {
            _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        }

        _repository.Verify(r => r.DeleteTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTeamMember_WhenMemberNotFound_ShouldThrowAndAbort()
    {
        // Arrange
        int idInexistente = 999;

        _repository
            .Setup(r => r.GetTeamMemberByIdAsync(idInexistente))
            .ReturnsAsync((MeuCrudCsharp.Models.TeamMember)null!);

        // Act & Assert
        var excecao = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.DeleteTeamMemberAsync(idInexistente)
        );

        Assert.Equal($"Membro {idInexistente} não encontrado.", excecao.Message);

        _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        _repository.Verify(
            r => r.DeleteTeamMemberAsync(It.IsAny<MeuCrudCsharp.Models.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
