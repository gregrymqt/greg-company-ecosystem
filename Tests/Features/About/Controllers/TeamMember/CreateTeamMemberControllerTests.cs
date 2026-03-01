using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Tests.Features.About;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.Features.About.Controllers.TeamMember;

public class CreateTeamMemberControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task CreateTeamMember_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();

        _controller.ModelState.AddModelError("Name", "O nome é obrigatório");

        // Act
        var result = await _controller.CreateTeamMember(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        _serviceMock.Verify(
            s => s.CreateTeamMemberAsync(It.IsAny<CreateUpdateTeamMemberDto>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateTeamMember_WhenUploadIsChunk_ShouldReturnOkWithMessage()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: true);
        dto.ChunkIndex = 1;

        _serviceMock.Setup(s => s.CreateTeamMemberAsync(dto)).ReturnsAsync((TeamMemberDto)null!);

        // Act
        var result = await _controller.CreateTeamMember(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(okResult.Value);
        var messageProperty = okResult.Value.GetType().GetProperty("message");
        var messageValue = messageProperty?.GetValue(okResult.Value)?.ToString();

        Assert.Equal($"Chunk {dto.ChunkIndex} recebido.", messageValue);
    }

    [Fact]
    public async Task CreateTeamMember_WhenCreationCompletes_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);
        var expectedDto = new TeamMemberDto();

        _serviceMock.Setup(s => s.CreateTeamMemberAsync(dto)).ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateTeamMember(dto);

        // Assert
        var createdResultAction = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetAboutPageContent", createdResultAction.ActionName);
        Assert.Equal(expectedDto, createdResultAction.Value);
    }

    [Theory]
    [InlineData(typeof(ResourceNotFoundException), 404)]
    [InlineData(typeof(Exception), 500)]
    public async Task CreateTeamMember_WhenExceptionOccurs_ShouldReturnHandledStatusCode(
        Type exceptionType,
        int expectedStatusCode
    )
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Erro simulado")!;

        _serviceMock.Setup(s => s.CreateTeamMemberAsync(dto)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CreateTeamMember(dto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
    }
}
