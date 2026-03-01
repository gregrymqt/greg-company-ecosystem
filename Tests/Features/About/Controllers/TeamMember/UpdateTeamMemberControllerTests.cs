using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Tests.Features.About;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.Features.About.Controllers.TeamMember;

public class UpdateTeamMemberControllerTests : AboutControllerTestBase
{
    [Fact]
    public async Task UpdateTeamMember_QuandoUploadCompletoOuNormal_DeveRetornarNoContent()
    {
        // Arrange
        int id = 1;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        _serviceMock.Setup(s => s.UpdateTeamMemberAsync(id, dto)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateTeamMember(id, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(s => s.UpdateTeamMemberAsync(id, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMember_QuandoUploadForChunk_DeveRetornarOkComMensagem()
    {
        // Arrange
        int id = 1;
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
        Assert.Equal($"Chunk {dto.ChunkIndex} atualizado.", messageValue);
    }

    [Theory]
    [InlineData(typeof(ResourceNotFoundException), 404)]
    [InlineData(typeof(Exception), 500)]
    public async Task UpdateTeamMember_QuandoOcorrerExcecao_DeveRetornarStatusCodeTratado(
        Type exceptionType,
        int expectedStatusCode
    )
    {
        // Arrange
        int id = 1;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Erro simulado")!;

        _serviceMock.Setup(s => s.UpdateTeamMemberAsync(id, dto)).ThrowsAsync(exception);

        // Act
        var result = await _controller.UpdateTeamMember(id, dto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
    }
}
