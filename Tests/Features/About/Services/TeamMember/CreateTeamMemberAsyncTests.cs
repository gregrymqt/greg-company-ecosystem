using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class CreateTeamMemberAsyncTests : AboutServiceTestBase // Focado no método de criação de membros
{
    [Fact]
    public async Task CreateTeamMemberAsync_WhenFileIsChunk_ShouldProcessChunkAndSaveFromTemp()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: true);

        _fileService
            .Setup(s =>
                s.ProcessChunkAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync("temp/caminho/foto.jpg");

        _fileService
            .Setup(s =>
                s.SalvarArquivoDoTempAsync("temp/caminho/foto.jpg", "foto.jpg", "AboutTeam")
            )
            .ReturnsAsync(AboutTestFakes.CreateFakeEntityFile());

        // Act
        var result = await _sut.CreateTeamMemberAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Lucas Vicente", result.Name);
        Assert.Equal("uploads/foto.jpg", result.PhotoUrl);

        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateTeamMemberAsync_WhenFileIsNotChunk_ShouldCallSalvarArquivoAsync()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        _fileService
            .Setup(s => s.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(AboutTestFakes.CreateFakeEntityFile());

        // Act
        var result = await _sut.CreateTeamMemberAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Lucas Vicente", result.Name);
        Assert.Equal("uploads/foto.jpg", result.PhotoUrl);

        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateTeamMemberAsync_WhenFileServiceFails_ShouldThrowExceptionAndNotCommit()
    {
        // Arrange
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false); // Fluxo normal

        // Mudamos o Setup para o método correto que será chamado
        _fileService
            .Setup(s => s.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Erro simulado no upload do arquivo"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateTeamMemberAsync(dto));

        Assert.Equal("Erro simulado no upload do arquivo", exception.Message);

        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
