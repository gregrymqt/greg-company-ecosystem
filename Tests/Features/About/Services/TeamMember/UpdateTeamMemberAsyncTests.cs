using System;
using MeuCrudCsharp.Features.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class UpdateTeamMemberAsyncTests : AboutServiceTestBase
{
    [Theory]
    [InlineData(false, false)] // Cenário 1: DTO sem arquivo (apenas atualiza texto)
    [InlineData(true, true)] // Cenário 2: DTO com arquivo, membro JÁ tinha foto (Substitui)
    [InlineData(true, false)] // Cenário 3: DTO com arquivo, membro NÃO tinha foto (Salva novo)
    public async Task UpdateTeamMember_CaminhosDeSucesso_DeveAtualizarCorretamente(
        bool enviarArquivo,
        bool jaTinhaFoto
    )
    {
        // Arrange
        int id = 1;
        // Se já tinha foto, o FileId é 10. Se não, é null.
        int? fileId = jaTinhaFoto ? 10 : null;

        var entity = CreateFakeTeamMemberEntity(fileId: fileId);
        var dto = CreateFakeTeamMemberDto();

        // Se o cenário não envia arquivo, forçamos o File a ser null
        if (!enviarArquivo)
        {
            dto.File = null;
        }

        var arquivoFake = CreateFakeEntityFile();

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);

        // Configurando os Mocks do FileService dependendo do cenário
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

        // Validações específicas da lógica de arquivos
        if (enviarArquivo)
        {
            // Se enviou arquivo, a entidade DEVE ter recebido os dados novos
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
            // Se não enviou arquivo, NUNCA deve chamar os serviços de arquivo
            _fileService.Verify(
                f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
                Times.Never
            );
            _fileService.Verify(
                f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never
            );
        }

        // Validações comuns a TODOS os cenários de sucesso (Sempre devem rodar)
        _repository.Verify(r => r.UpdateTeamMemberAsync(entity), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMember_QuandoMembroNaoExiste_DeveLancarExcecaoEAbortar()
    {
        // Arrange
        int idInexistente = 999;
        var dto = CreateFakeTeamMemberDto();

        _repository
            .Setup(r => r.GetTeamMemberByIdAsync(idInexistente))
            .ReturnsAsync((MeuCrudCsharp.Models.TeamMember)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.UpdateTeamMemberAsync(idInexistente, dto)
        );

        // A regra de ouro das exceptions: garantir que nada vazou!
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
    [InlineData(true)] // Cenário 1: Falha ao substituir um arquivo que já existia
    [InlineData(false)] // Cenário 2: Falha ao salvar o primeiro arquivo do membro
    public async Task UpdateTeamMember_QuandoFileServiceFalhar_DeveSubirExcecaoEAbortar(
        bool jaTinhaFoto
    )
    {
        // Arrange
        int id = 1;
        // Se a variável do InlineData for true, colocamos o FileId = 10, senão, null
        int? fileId = jaTinhaFoto ? 10 : null;

        var entity = CreateFakeTeamMemberEntity(fileId);
        var dto = CreateFakeTeamMemberDto(); // O DTO vem com arquivo para forçar a entrada no if(dto.File != null)

        _repository.Setup(r => r.GetTeamMemberByIdAsync(id)).ReturnsAsync(entity);

        // Plantando a bomba de acordo com o cenário do InlineData!
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

        // Independentemente de qual caminho falhou, o banco NUNCA pode ser atualizado
        _repository.Verify(
            r => r.UpdateTeamMemberAsync(It.IsAny<MeuCrudCsharp.Models.TeamMember>()),
            Times.Never
        );
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
