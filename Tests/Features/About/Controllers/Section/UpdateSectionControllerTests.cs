using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Tests.Features.About;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

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
        Assert.Equal($"Chunk {dto.ChunkIndex} atualizado.", messageValue);
    }

    [Theory]
    [InlineData(typeof(ResourceNotFoundException), 404)]
    [InlineData(typeof(Exception), 500)]
    public async Task UpdateSection_WhenExceptionOccurs_ShouldReturnHandledStatusCode(
        Type exceptionType,
        int expectedStatusCode
    )
    {
        // Arrange
        int sectionId = 1;
        var dto = AboutTestFakes.CreateFakeAboutSectionDto();

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Erro simulado")!;

        _serviceMock.Setup(s => s.UpdateSectionAsync(sectionId, dto)).ThrowsAsync(exception);

        // Act
        var result = await _controller.UpdateSection(sectionId, dto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
    }
}
