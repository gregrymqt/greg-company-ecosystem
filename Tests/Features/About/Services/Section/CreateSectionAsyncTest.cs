using System;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.Section;

public class CreateSectionAsyncTest : AboutServiceTestBase
{
    [Theory]
    [InlineData(false, true)] // Cenário 1: Upload Normal (Arquivo Pequeno)
    [InlineData(true, true)] // Cenário 2: Chunking (Último pedaço, arquivo completo)
    [InlineData(false, false)] // Cenário 3: Criação sem nenhum arquivo
    public async Task CreateSection_CaminhosDeSucesso_DeveSalvarEntidade(bool isChunk, bool hasFile)
    {
        // Arrange
        var dto = CreateFakeAboutSectionDto(isChunk);
        dto.File = hasFile ? new Mock<IFormFile>().Object : null;
        dto.FileName = hasFile ? "foto.jpg" : null;

        // Configurando os Mocks do FileService dependendo do cenário do InlineData
        if (isChunk && hasFile)
        {
            // Simula que o ProcessChunk retornou o caminho temporário (ou seja, completou)
            _fileService
                .Setup(f =>
                    f.ProcessChunkAsync(dto.File, dto.FileName, It.IsAny<int>(), It.IsAny<int>())
                )
                .ReturnsAsync("/temp/path/foto.jpg");

            // Simula o salvamento final do arquivo que estava no temp
            _fileService
                .Setup(f =>
                    f.SalvarArquivoDoTempAsync(
                        "/temp/path/foto.jpg",
                        dto.FileName,
                        It.IsAny<string>()
                    )
                )
                .ReturnsAsync(CreateFakeEntityFile());
        }
        else if (!isChunk && hasFile)
        {
            // Simula o upload normal
            _fileService
                .Setup(f => f.SalvarArquivoAsync(dto.File, It.IsAny<string>()))
                .ReturnsAsync(CreateFakeEntityFile());
        }

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        Assert.NotNull(result); // Garante que não retornou nulo
        Assert.Equal(dto.Title, result.Title); // Garante que mapeou o DTO pra Entidade

        // Verificações essenciais de banco e cache (devem rodar em TODOS esses 3 cenários)
        _repository.Verify(r => r.AddSectionAsync(It.IsAny<AboutSection>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);

        // Verificações específicas de arquivo dependendo do cenário
        if (isChunk && hasFile)
        {
            _fileService.Verify(
                f =>
                    f.ProcessChunkAsync(
                        It.IsAny<IFormFile>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()
                    ),
                Times.Once
            );
            _fileService.Verify(
                f =>
                    f.SalvarArquivoDoTempAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    ),
                Times.Once
            );
        }
        else if (!isChunk && hasFile)
        {
            _fileService.Verify(
                f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Once
            );
        }
        else
        {
            // Se não tem arquivo, NÃO deve ter chamado nenhum serviço de arquivo
            _fileService.Verify(
                f => f.SalvarArquivoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never
            );
            _fileService.Verify(
                f =>
                    f.ProcessChunkAsync(
                        It.IsAny<IFormFile>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()
                    ),
                Times.Never
            );
        }
    }

    [Fact]
    public async Task CreateSection_QuandoChunkNaoEstaCompleto_DeveRetornarNullENaoSalvarNoBanco()
    {
        // Arrange
        var dto = CreateFakeAboutSectionDto(isChunk: true);

        // Simula que ainda faltam pedaços (retorna null)
        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(dto.File, dto.FileName, It.IsAny<int>(), It.IsAny<int>())
            )
            .ReturnsAsync((string)null!);

        // Act
        var result = await _sut.CreateSectionAsync(dto);

        // Assert
        Assert.Null(result); // O DTO retornado deve ser nulo

        // A Malícia: Garante que o fluxo foi interrompido antes de ir pro banco
        _fileService.Verify(
            f =>
                f.SalvarArquivoDoTempAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
        _repository.Verify(r => r.AddSectionAsync(It.IsAny<AboutSection>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }
}
