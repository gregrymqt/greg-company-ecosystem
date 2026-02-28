using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;

namespace Tests.Features.About.Services;

using Moq;

public abstract class AboutServiceTestBase
{
    // Usamos 'protected' para as classes que herdarem conseguirem usar
    protected readonly Mock<IAboutRepository> _repository;
    protected readonly Mock<IFileService> _fileService;
    protected readonly Mock<IUnitOfWork> _unitOfWork;
    protected readonly Mock<ICacheService> _cache;
    
    // O seu Service pronto para ser testado
    protected readonly AboutService _sut;

    protected AboutServiceTestBase()
    {
        // Inicializa todos os mocks do zero para cada teste
        _repository = new Mock<IAboutRepository>();
        _fileService = new Mock<IFileService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cache = new Mock<ICacheService>();

        // Instancia o serviço injetando os objetos falsos
        _sut = new AboutService(
            _repository.Object,
            _cache.Object,
            _fileService.Object,
            _unitOfWork.Object
        );
    }
}