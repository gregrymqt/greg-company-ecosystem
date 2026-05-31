using MeuCrudCsharp.Features.About.Application.DTOs;
using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Features.About.Controllers.TeamMember;
public class CreateTeamMemberControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task CreateTeamMember_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();

        // Traduzido para inglês
        _controller.ModelState.AddModelError("Name", "Name is required");

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

        // Traduzido para inglês ("received" no lugar de "recebido")
        Assert.Equal($"Chunk {dto.ChunkIndex} received.", messageValue);
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

    [Fact]
    public async Task CreateTeamMember_WhenResourceNotFoundExceptionOccurs_ShouldReturn404()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();

        _serviceMock
            .Setup(s => s.CreateTeamMemberAsync(dto))
            // Traduzido para inglês
            .ThrowsAsync(new ResourceNotFoundException("Simulated error"));

        // Act
        var result = await _controller.CreateTeamMember(dto);

        // Assert
        // Corrigido para NotFoundObjectResult
        var objectResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateTeamMember_WhenUnhandledExceptionOccurs_ShouldReturn500()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();

        _serviceMock
            .Setup(s => s.CreateTeamMemberAsync(dto))
            // Traduzido para inglês
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var result = await _controller.CreateTeamMember(dto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}