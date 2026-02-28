using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Moq;

namespace Tests.Features.About.Services.Section;

public class DeleteAboutAsyncTest : AboutServiceTestBase
{

    [Theory]
    [InlineData(null)]
    [InlineData(10)]
    public async Task DeleteSection_DeveExecutarFluxoCompleto(int? fileId)
    {
        // Arrange
        var entity = CreateFakeSectionEntity(fileId);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

        _repository
            .Setup(r => r.DeleteSectionAsync(It.IsAny<AboutSection>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteSectionAsync(entity.Id);

        // Assert

        // 1. Verifica se salvou no banco
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);

        // 2. Verifica se limpou o cache
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);

        // 3. A LÓGICA DO ARQUIVO (O "Pulo do Gato"):
        if (fileId.HasValue)
        {
            // Se tem ID, DEVE ter chamado a deleção do arquivo uma vez
            _fileService.Verify(f => f.DeletarArquivoAsync(fileId.Value), Times.Once);
        }
        else
        {
            // Se NÃO tem ID, NUNCA deve ter chamado o serviço de arquivo
            _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        }
    }

    [Fact]
    public async Task DeleteSection_QuandoNaoEncontraEntidade_DeveLancarExcecao()
    {
        // Arrange
        int idInexistente = 999; // Criamos um ID real para simular

        _repository
            // Usamos o ID falso aqui
            .Setup(r => r.GetSectionByIdAsync(idInexistente))
            .ReturnsAsync((AboutSection)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            // Passamos o ID falso para o método real
            _sut.DeleteSectionAsync(idInexistente)
        );

        // Assert: Garantindo que nada mais foi chamado
        _fileService.Verify(f => f.DeletarArquivoAsync(It.IsAny<int>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
