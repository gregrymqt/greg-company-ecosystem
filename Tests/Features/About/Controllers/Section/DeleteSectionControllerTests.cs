using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

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

    [Theory]
    [InlineData(typeof(ResourceNotFoundException), 404)]
    [InlineData(typeof(Exception), 500)]
    public async Task DeleteSection_WhenExceptionOccurs_ShouldReturnHandledStatusCode(
        Type exceptionType,
        int expectedStatusCode
    )
    {
        // Arrange
        int sectionId = 1;

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Erro simulado")!;

        _serviceMock.Setup(s => s.DeleteSectionAsync(sectionId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.DeleteSection(sectionId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
    }
}
