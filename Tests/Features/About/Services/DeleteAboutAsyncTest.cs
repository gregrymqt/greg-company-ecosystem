using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Moq;

namespace Tests.Features.About.Services;

public class DeleteAboutAsyncTest
{
    private readonly Mock<IAboutRepository> _repository;
    private readonly Mock<ICacheService> _cache;
    private readonly Mock<IFileService> _fileService;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    private readonly IAboutService _sup;

    public DeleteAboutAsyncTest()
    {
        _repository = new Mock<IAboutRepository>();
        _cache = new Mock<ICacheService>();
        _fileService = new Mock<IFileService>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _sup = new AboutService(
            _repository.Object,
            _cache.Object,
            _fileService.Object,
            _unitOfWork.Object);
    }

    private AboutSection CreateFakeEntity(int? fileId = null) => new()
    {
        Id = 1, Title = "Velho", FileId = fileId
    };

    [Theory]
    [InlineData(null)]
    [InlineData(10)]
    public async Task DeleteSection_DeveExecutarFluxoCompleto(int? fileId)
    {
        // Arrange
        var entity = CreateFakeEntity(fileId);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(entity);

        _repository.Setup(r => r.DeleteSectionAsync(It.IsAny<AboutSection>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sup.DeleteSectionAsync(entity.Id);

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
}