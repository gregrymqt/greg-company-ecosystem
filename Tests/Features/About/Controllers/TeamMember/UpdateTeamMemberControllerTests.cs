using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Features.About;

namespace Tests.Features.About.Controllers.TeamMember;
public class UpdateTeamMemberControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task UpdateTeamMember_WhenUploadIsCompleteOrNormal_ShouldReturnNoContent()
    {
        // Arrange
        const int id = 1;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        _serviceMock.Setup(s => s.UpdateTeamMemberAsync(id, dto)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateTeamMember(id, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(s => s.UpdateTeamMemberAsync(id, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenUploadIsChunk_ShouldReturnOkWithMessage()
    {
        // Arrange
        const int id = 1;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: true);
        dto.ChunkIndex = 2;

        _serviceMock.Setup(s => s.UpdateTeamMemberAsync(id, dto)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateTeamMember(id, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(okResult.Value);
        var messageProperty = okResult.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);

        var messageValue = messageProperty.GetValue(okResult.Value)?.ToString();
        Assert.Equal($"Chunk {dto.ChunkIndex} updated.", messageValue);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenResourceNotFoundExceptionOccurs_ShouldReturn404()
    {
        // Arrange
        const int id = 1;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();

        _serviceMock
            .Setup(s => s.UpdateTeamMemberAsync(id, dto))
            .ThrowsAsync(new ResourceNotFoundException("Simulated error"));

        // Act
        var result = await _controller.UpdateTeamMember(id, dto);

        // Assert
        // Corrigido para NotFoundObjectResult
        var objectResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenUnhandledExceptionOccurs_ShouldReturn500()
    {
        // Arrange
        const int id = 1;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();

        _serviceMock
            .Setup(s => s.UpdateTeamMemberAsync(id, dto))
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var result = await _controller.UpdateTeamMember(id, dto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}