using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.TeamMember;

public class CreateTeamMemberAsyncTests : AboutServiceTestBase // Focado no método de criação de membros
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateTeamMemberAsync_WhenFileIsChunk_ShouldReturnDtoAndSaveCorrectly(
        bool isChunk
    )
    {
        // 1. Arrange (Preparar)
        var dto = CreateFakeTeamMemberDto(isChunk);

        // NÃO chamamos o serviço real. APENAS dizemos o que o Mock deve retornar.
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
            .ReturnsAsync(CreateFakeEntityFile());

        // 2. Act (Agir)
        var result = await _sut.CreateTeamMemberAsync(dto);

        // 3. Assert (Verificar)
        Assert.NotNull(result);
        Assert.Equal("Lucas Vicente", result.Name);
        Assert.Equal("uploads/foto.jpg", result.PhotoUrl);

        // Verifica se o banco e o cache foram acionados
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateTeamMemberAsync_WhenFileServiceFails_ShouldThrowExceptionAndNotCommit()
    {
        // 1. Arrange (Preparar)
        // Reutilizamos o seu método auxiliar para criar o DTO
        var dto = CreateFakeTeamMemberDto(false);

        // Aqui está o "pulo do gato": Forçamos o Mock do serviço de arquivo a falhar!
        // Em vez de .ReturnsAsync, usamos .ThrowsAsync
        _fileService
            .Setup(s =>
                s.ProcessChunkAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ThrowsAsync(new Exception("Erro simulado no upload do arquivo"));

        // 2 & 3. Act & Assert (Agir e Verificar JUNTOS)
        // Executamos o método DENTRO do Assert usando () =>
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateTeamMemberAsync(dto));

        // (Opcional) Podemos verificar se a mensagem da exceção é exatamente a que esperamos
        Assert.Equal("Erro simulado no upload do arquivo", exception.Message);

        // 4. Verificações de segurança (Garantir que nada foi salvo pela metade)
        // O banco NÃO pode ter sido commitado
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);

        // O cache NÃO pode ter sido limpo
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
