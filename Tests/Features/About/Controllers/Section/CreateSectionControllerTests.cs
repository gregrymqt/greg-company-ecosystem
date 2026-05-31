using MeuCrudCsharp.Features.About.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
        // Traduzido para inglês ("received" no lugar de "recebido")
        Assert.Contains("Chunk 2 received.", responseValue);
    }

    [Fact]
    public async Task CreateSection_WhenModelStateIsInvalid_ShouldReturn400BadRequest()
    {
        // Arrange
        var dto = new CreateUpdateAboutSectionDto();

        // Traduzido para inglês
        _controller.ModelState.AddModelError("Title", "Title is required");

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
            // Traduzido para inglês
            .ThrowsAsync(new Exception("Simulated disk error"));

        // Act
        var result = await _controller.CreateSection(dto);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);

        var responseValue = serverErrorResult.Value?.ToString();
        // Traduzido para inglês
        Assert.Contains("Error creating section.", responseValue);
    }
}
