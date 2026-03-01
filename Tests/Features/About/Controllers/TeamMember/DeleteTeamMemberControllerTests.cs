using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

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

    [Theory]
    [InlineData(typeof(ResourceNotFoundException), 404)]
    [InlineData(typeof(Exception), 500)]
    public async Task DeleteTeamMember_WhenExceptionOccurs_ShouldReturnHandledStatusCode(
        Type exceptionType,
        int expectedStatusCode
    )
    {
        // Arrange
        int id = 1;

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Erro simulado")!;

        _serviceMock.Setup(s => s.DeleteTeamMemberAsync(id)).ThrowsAsync(exception);

        // Act
        var result = await _controller.DeleteTeamMember(id);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
    }
}
