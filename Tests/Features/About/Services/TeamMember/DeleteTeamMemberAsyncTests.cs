using System;
using MeuCrudCsharp.Features.Exceptions;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class DeleteTeamMemberAsyncTests : AboutServiceTestBase
{
    [Theory]
    [InlineData(10)] // Cenário 1: O membro possui um arquivo associado (FileId = 10)
    [InlineData(null)] // Cenário 2: O membro não possui arquivo associado (FileId = null)
    public async Task DeleteTeamMember_CaminhosDeSucesso_DeveDeletarMembroEArquivoSeExistir(
        int? fileId
    )
    {
        // Arrange
        int idMembro = 1;

        // Criamos a entidade falsa repassando o fileId (que pode ser 10 ou null, dependendo do InlineData)
        var entity = CreateFakeTeamMemberEntity(fileId: fileId);

        _repository.Setup(r => r.GetTeamMemberByIdAsync(idMembro)).ReturnsAsync(entity);

        // Act
        await _sut.DeleteTeamMemberAsync(idMembro);

        // Assert

        // 1. Validação condicional do arquivo
        if (fileId.HasValue)
        {
            // Se existia foto, o DeletarArquivoAsync DEVE ter sido chamado com o ID correto
            _fileService.Verify(f => f.DeletarArquivoAsync(fileId.Value), Times.Once);
        }
        else
        {
            // Se NÃO existia foto, o DeletarArquivoAsync NUNCA pode ter sido chamado
            _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        }

        // 2. Validações fixas (Sempre devem acontecer nos cenários de sucesso)
        _repository.Verify(r => r.DeleteTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTeamMember_QuandoMembroNaoExiste_DeveLancarExcecaoEAbortar()
    {
        // Arrange
        int idInexistente = 999;

        // Simulamos o repositório não encontrando o membro no banco
        _repository
            .Setup(r => r.GetTeamMemberByIdAsync(idInexistente))
            .ReturnsAsync((MeuCrudCsharp.Models.TeamMember)null!);

        // Act & Assert
        // Capturamos a ResourceNotFoundException na nossa armadilha
        var excecao = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.DeleteTeamMemberAsync(idInexistente)
        );

        // Podemos validar até se a mensagem do erro está exatamente igual à do seu código
        Assert.Equal($"Membro {idInexistente} não encontrado.", excecao.Message);

        // Verificações vitais: A missão DEVE ser abortada
        // Nada de deletar arquivo fantasma, nada de deletar no banco e nada de commitar
        _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        _repository.Verify(
            r => r.DeleteTeamMemberAsync(It.IsAny<MeuCrudCsharp.Models.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
