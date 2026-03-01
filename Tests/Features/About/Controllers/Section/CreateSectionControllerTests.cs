using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Tests.Features.About;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Features.About.Services;
using Xunit;

namespace Tests.Features.About.Controllers.Section;

public class CreateSectionControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task CreateSection_WhenUploadIsComplete_ShouldReturn201Created()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto();
        var expectedDto = new AboutSectionDto();

        _serviceMock.Setup(s => s.CreateSectionAsync(dto)).ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateSection(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("GetAboutPageContent", createdResult.ActionName);
        Assert.Equal(expectedDto, createdResult.Value);
    }

    [Fact]
    public async Task CreateSection_WhenChunkReceived_ShouldReturn200OkWithMessage()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: true);
        dto.ChunkIndex = 2;
        dto.TotalChunks = 5;

        _serviceMock.Setup(s => s.CreateSectionAsync(dto)).ReturnsAsync((AboutSectionDto?)null);

        // Act
        var result = await _controller.CreateSection(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var responseValue = okResult.Value?.ToString();
        Assert.Contains("Chunk 2 recebido.", responseValue);
    }

    [Fact]
    public async Task CreateSection_WhenModelStateIsInvalid_ShouldReturn400BadRequest()
    {
        // Arrange
        var dto = new CreateUpdateAboutSectionDto();

        _controller.ModelState.AddModelError("Title", "O título é obrigatório");

        // Act
        var result = await _controller.CreateSection(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);

        _serviceMock.Verify(
            s => s.CreateSectionAsync(It.IsAny<CreateUpdateAboutSectionDto>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateSection_WhenServiceFails_ShouldReturn500InternalServerError()
    {
        // Arrange
        var dto = new CreateUpdateAboutSectionDto();

        _serviceMock
            .Setup(s => s.CreateSectionAsync(dto))
            .ThrowsAsync(new Exception("Erro simulado no disco"));

        // Act
        var result = await _controller.CreateSection(dto);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);

        var responseValue = serverErrorResult.Value?.ToString();
        Assert.Contains("Erro ao criar a seção.", responseValue);
    }
}
