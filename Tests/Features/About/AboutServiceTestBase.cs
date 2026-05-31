using MeuCrudCsharp.Features.About.Domain.Interfaces;
using MeuCrudCsharp.Features.About.Application.Interfaces;
using MeuCrudCsharp.Features.About.Application.Services;
using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Files.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using Moq;

namespace Tests.Features.About;

public abstract class AboutServiceTestBase
{
    protected readonly Mock<IAboutRepository> _repository;
    protected readonly Mock<IFileService> _fileService;
    protected readonly Mock<IUnitOfWork> _unitOfWork;
    protected readonly Mock<ICacheService> _cache;

    protected readonly AboutService _sut;

    protected AboutServiceTestBase()
    {
        _repository = new Mock<IAboutRepository>();
        _fileService = new Mock<IFileService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cache = new Mock<ICacheService>();

        _sut = new AboutService(
            _repository.Object,
            _cache.Object,
            _fileService.Object,
            _unitOfWork.Object
        );
    }
}
