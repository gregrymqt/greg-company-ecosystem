using MeuCrudCsharp.Features.About.Controllers;
using MeuCrudCsharp.Features.About.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Features.About.Controllers;

public abstract class AboutControllerTestBase
{
    protected readonly Mock<IAboutService> _serviceMock;
    protected readonly AboutController _controller;

    protected AboutControllerTestBase()
    {
        _serviceMock = new Mock<IAboutService>();
        _controller = new AboutController(_serviceMock.Object);
    }
}
