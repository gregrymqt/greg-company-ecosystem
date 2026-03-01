using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.Features.About.Controllers;

public class GetAboutControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task GetAboutPageContent_Sucesso_DeveRetornar200OkComOsDados()
    {
        var contentFake = new AboutPageContentDto();

        _serviceMock.Setup(s => s.GetAboutPageContentAsync()).ReturnsAsync(contentFake);

        var result = await _controller.GetAboutPageContent();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(contentFake, okResult.Value);
    }

    [Fact]
    public async Task GetAboutPageContent_FalhaNoServico_DeveRetornar500InternalServerError()
    {
        _serviceMock
            .Setup(s => s.GetAboutPageContentAsync())
            .ThrowsAsync(new Exception("Falha de conexão com o banco de dados"));

        var result = await _controller.GetAboutPageContent();

        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);

        var responseValue = serverErrorResult.Value?.ToString();
        Assert.Contains("Erro ao carregar a página Sobre.", responseValue);
    }
}
