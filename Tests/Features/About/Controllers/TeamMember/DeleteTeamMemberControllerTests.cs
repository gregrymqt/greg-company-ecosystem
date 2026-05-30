using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Features.About.Controllers.TeamMember;
public class DeleteTeamMemberControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task DeleteTeamMember_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        int id = 1;

        _serviceMock.Setup(s => s.DeleteTeamMemberAsync(id)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteTeamMember(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(s => s.DeleteTeamMemberAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteTeamMember_WhenResourceNotFoundExceptionOccurs_ShouldReturn404()
    {
        // Arrange
        int id = 1;

        _serviceMock
            .Setup(s => s.DeleteTeamMemberAsync(id))
            // Traduzido para inglês
            .ThrowsAsync(new ResourceNotFoundException("Simulated error"));

        // Act
        var result = await _controller.DeleteTeamMember(id);

        // Assert
        // Corrigido para NotFoundObjectResult
        var objectResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteTeamMember_WhenUnhandledExceptionOccurs_ShouldReturn500()
    {
        // Arrange
        int id = 1;

        _serviceMock
            .Setup(s => s.DeleteTeamMemberAsync(id))
            // Traduzido para inglês
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var result = await _controller.DeleteTeamMember(id);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}