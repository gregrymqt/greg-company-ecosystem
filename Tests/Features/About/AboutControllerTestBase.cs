using MeuCrudCsharp.Features.About.Presentation.Controllers;
using MeuCrudCsharp.Features.About.Application.Interfaces;
using Moq;

namespace Tests.Features.About;

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
