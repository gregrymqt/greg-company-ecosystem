using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class UpdateTeamMemberAsyncTests : AboutServiceTestBase
{
    [Fact]
    public async Task UpdateTeamMember_WithTextOnly_ShouldUpdateDataAndNotCallFileService()
    {
        // Arrange
        const int id = 1;
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: 10); // Assume it has a photo
        var dto = AboutTestFakes.CreateFakeTeamMemberDto();
        dto.File = null; // No file in DTO

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);

        // Act
        var result = await _sut.UpdateTeamMemberAsync(id, dto);

        // Assert
        Assert.True(result);
        Assert.Equal(10, entity.FileId); // FileId should not change

        _fileService.Verify(
            f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
            Times.Never
        );
        _fileService.Verify(
            f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
            Times.Never
        );
        _repository.Verify(r => r.UpdateTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenAddingFirstPhoto_ShouldCallSaveFileService()
    {
        // Arrange
        const int id = 1;
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: null); // No photo initially
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(); // DTO has a file
        var fakeFile = AboutTestFakes.CreateFakeEntityFile();

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);
        _fileService
            .Setup(f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()))
            .ReturnsAsync(fakeFile);

        // Act
        var result = await _sut.UpdateTeamMemberAsync(id, dto);

        // Assert
        Assert.True(result);
        Assert.Equal(fakeFile.Id, entity.FileId);
        Assert.Equal(fakeFile.CaminhoRelativo, entity.PhotoUrl);

        _fileService.Verify(f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()), Times.Once);
        _fileService.Verify(
            f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
            Times.Never
        );
        _repository.Verify(r => r.UpdateTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenReplacingPhoto_ShouldCallReplaceFileService()
    {
        // Arrange
        const int id = 1;
        const int existingFileId = 10;
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: existingFileId); // Has a photo
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(); // DTO has a file
        var newFakeFile = AboutTestFakes.CreateFakeEntityFile();
        newFakeFile.Id = 11; // Different ID for the new file

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);
        _fileService
            .Setup(f => f.SubstituirArquivoAsync(existingFileId, dto.File))
            .ReturnsAsync(newFakeFile);

        // Act
        var result = await _sut.UpdateTeamMemberAsync(id, dto);

        // Assert
        Assert.True(result);
        Assert.Equal(newFakeFile.Id, entity.FileId);
        Assert.Equal(newFakeFile.CaminhoRelativo, entity.PhotoUrl);

        _fileService.Verify(f => f.SubstituirArquivoAsync(existingFileId, dto.File), Times.Once);
        _fileService.Verify(
            f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
            Times.Never
        );
        _repository.Verify(r => r.UpdateTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenMemberNotFound_ShouldThrowAndAbort()
    {
        // Arrange
        const int idInexistente = 999;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        _repository
            .Setup(r => r.GetTeamMemberByIdAsync(idInexistente))
            .ReturnsAsync((MeuCrudCsharp.Features.About.Domain.Entities.TeamMember)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.UpdateTeamMemberAsync(idInexistente, dto)
        );

        _fileService.Verify(
            f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
            Times.Never
        );
        _fileService.Verify(
            f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
            Times.Never
        );
        _repository.Verify(
            r => r.UpdateTeamMemberAsync(It.IsAny<MeuCrudCsharp.Features.About.Domain.Entities.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenMemberAlreadyHasFileAndServiceFails_ShouldThrowAndAbort()
    {
        // Arrange
        const int id = 1;
        const int fileId = 10;
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId);
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);
        _fileService
            .Setup(f => f.SubstituirArquivoAsync(entity.FileId!.Value, dto.File))
            .ThrowsAsync(new Exception("Simulated replacement failure"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateTeamMemberAsync(id, dto));

        _repository.Verify(
            r => r.UpdateTeamMemberAsync(It.IsAny<MeuCrudCsharp.Features.About.Domain.Entities.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenMemberHasNoFileAndServiceFails_ShouldThrowAndAbort()
    {
        // Arrange
        const int id = 1;
        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: null);
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);
        _fileService
            .Setup(f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()))
            .ThrowsAsync(new Exception("Simulated new save failure"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateTeamMemberAsync(id, dto));

        _repository.Verify(
            r => r.UpdateTeamMemberAsync(It.IsAny<MeuCrudCsharp.Features.About.Domain.Entities.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
