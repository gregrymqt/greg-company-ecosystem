using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Features.About.Controllers.Section;

public class UpdateSectionControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task UpdateSection_WhenUploadIsCompleteOrNormal_ShouldReturnNoContent()
    {
        // Arrange
        int sectionId = 1;
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: false);

        _serviceMock.Setup(s => s.UpdateSectionAsync(sectionId, dto)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateSection(sectionId, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(s => s.UpdateSectionAsync(sectionId, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateSection_WhenUploadIsIntermediaryChunk_ShouldReturnOkWithMessage()
    {
        // Arrange
        int sectionId = 1;
        var dto = AboutTestFakes.CreateFakeAboutSectionDto(isChunk: true);
        dto.ChunkIndex = 2;

        _serviceMock.Setup(s => s.UpdateSectionAsync(sectionId, dto)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateSection(sectionId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(okResult.Value);
        var messageProperty = okResult.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);

        var messageValue = messageProperty.GetValue(okResult.Value)?.ToString();
        // Traduzido para inglês ("updated" no lugar de "atualizado")
        Assert.Equal($"Chunk {dto.ChunkIndex} updated.", messageValue);
    }

    [Fact]
    public async Task UpdateSection_WhenResourceNotFoundExceptionOccurs_ShouldReturn404()
    {
        // Arrange
        int sectionId = 1;
        var dto = AboutTestFakes.CreateFakeAboutSectionDto();

        _serviceMock
            .Setup(s => s.UpdateSectionAsync(sectionId, dto))
            // Traduzido para inglês
            .ThrowsAsync(new ResourceNotFoundException("Simulated error"));

        // Act
        var result = await _controller.UpdateSection(sectionId, dto);

        // Assert
        // Corrigido de ObjectResult para NotFoundObjectResult
        var objectResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateSection_WhenUnhandledExceptionOccurs_ShouldReturn500()
    {
        // Arrange
        int sectionId = 1;
        var dto = AboutTestFakes.CreateFakeAboutSectionDto();

        _serviceMock
            .Setup(s => s.UpdateSectionAsync(sectionId, dto))
            // Traduzido para inglês
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var result = await _controller.UpdateSection(sectionId, dto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
