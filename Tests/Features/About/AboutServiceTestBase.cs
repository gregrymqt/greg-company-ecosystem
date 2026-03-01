using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;

namespace Tests.Features.About.Services;

using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Http;
using Moq;

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
