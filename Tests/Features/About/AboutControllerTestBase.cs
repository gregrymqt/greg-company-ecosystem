using MeuCrudCsharp.Features.About.Controllers;
using MeuCrudCsharp.Features.About.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

public abstract class AboutControllerTestBase
{
    protected readonly Mock<IAboutService> _serviceMock;
    protected readonly AboutController _controller;

    protected AboutControllerTestBase()
    {
        _serviceMock = new Mock<IAboutService>();
        // Injetamos o mock falso na Controller real
        _controller = new AboutController(_serviceMock.Object);
    }
}
