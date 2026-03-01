using System;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Tests.Features.About;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class UpdateTeamMemberAsyncTests : AboutServiceTestBase
{
    [Theory]
    [InlineData(false, false)] // Scenario 1: DTO without file (text-only update)
    [InlineData(true, true)] // Scenario 2: DTO with file, member already had photo (Replace)
    [InlineData(true, false)] // Scenario 3: DTO with file, member had no photo (Save new)
    public async Task UpdateTeamMember_WhenSuccessfulPaths_ShouldUpdateCorrectly(
        bool enviarArquivo,
        bool jaTinhaFoto
    )
    {
        // Arrange
        int id = 1;
        int? fileId = jaTinhaFoto ? 10 : null;

        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId: fileId);
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        if (!enviarArquivo)
        {
            dto.File = null;
        }

        var arquivoFake = AboutTestFakes.CreateFakeEntityFile();

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);

        if (enviarArquivo && jaTinhaFoto)
        {
            _fileService
                .Setup(f => f.SubstituirArquivoAsync(entity.FileId.Value, dto.File))
                .ReturnsAsync(arquivoFake);
        }
        else if (enviarArquivo && !jaTinhaFoto)
        {
            _fileService
                .Setup(f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()))
                .ReturnsAsync(arquivoFake);
        }

        // Act
        var result = await _sut.UpdateTeamMemberAsync(id, dto);

        // Assert
        Assert.True(result);

        if (enviarArquivo)
        {
            Assert.Equal(arquivoFake.Id, entity.FileId);
            Assert.Equal(arquivoFake.CaminhoRelativo, entity.PhotoUrl);

            if (jaTinhaFoto)
            {
                _fileService.Verify(f => f.SubstituirArquivoAsync(10, dto.File), Times.Once);
                _fileService.Verify(
                    f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                    Times.Never
                );
            }
            else
            {
                _fileService.Verify(
                    f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()),
                    Times.Once
                );
                _fileService.Verify(
                    f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
                    Times.Never
                );
            }
        }
        else
        {
            _fileService.Verify(
                f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
                Times.Never
            );
            _fileService.Verify(
                f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never
            );
        }

        _repository.Verify(r => r.UpdateTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMember_WhenMemberNotFound_ShouldThrowAndAbort()
    {
        // Arrange
        int idInexistente = 999;
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false);

        _repository
            .Setup(r => r.GetTeamMemberByIdAsync(idInexistente))
            .ReturnsAsync((MeuCrudCsharp.Models.TeamMember)null!);

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
            r => r.UpdateTeamMemberAsync(It.IsAny<MeuCrudCsharp.Models.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData(true)] // Scenario 1: Failure when replacing an existing file
    [InlineData(false)] // Scenario 2: Failure when saving the first file for the member
    public async Task UpdateTeamMember_WhenFileServiceFails_ShouldThrowAndAbort(bool jaTinhaFoto)
    {
        // Arrange
        int id = 1;
        int? fileId = jaTinhaFoto ? 10 : null;

        var entity = AboutTestFakes.CreateFakeTeamMemberEntity(fileId);
        var dto = AboutTestFakes.CreateFakeTeamMemberDto(isChunk: false); 

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);

        if (jaTinhaFoto)
        {
            _fileService
                .Setup(f => f.SubstituirArquivoAsync(entity.FileId.Value, dto.File))
                .ThrowsAsync(new Exception("Falha simulada na substituição"));
        }
        else
        {
            _fileService
                .Setup(f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Falha simulada no salvamento novo"));
        }

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateTeamMemberAsync(id, dto));

        _repository.Verify(
            r => r.UpdateTeamMemberAsync(It.IsAny<MeuCrudCsharp.Models.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
