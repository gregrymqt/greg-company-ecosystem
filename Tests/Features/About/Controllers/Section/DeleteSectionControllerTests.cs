using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Features.About.Controllers.Section;

public class DeleteSectionControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task DeleteSection_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        int sectionId = 1;

        _serviceMock.Setup(s => s.DeleteSectionAsync(sectionId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteSection(sectionId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(s => s.DeleteSectionAsync(sectionId), Times.Once);
    }

    [Fact]
    public async Task DeleteSection_WhenResourceNotFoundExceptionOccurs_ShouldReturn404()
    {
        // Arrange
        int sectionId = 1;

        _serviceMock
            .Setup(s => s.DeleteSectionAsync(sectionId))
            // Traduzido para inglês
            .ThrowsAsync(new ResourceNotFoundException("Simulated error"));

        // Act
        var result = await _controller.DeleteSection(sectionId);

        // Assert
        // Corrigido de ObjectResult para NotFoundObjectResult
        var objectResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteSection_WhenUnhandledExceptionOccurs_ShouldReturn500()
    {
        // Arrange
        int sectionId = 1;

        _serviceMock
            .Setup(s => s.DeleteSectionAsync(sectionId))
            // Traduzido para inglês
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var result = await _controller.DeleteSection(sectionId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
