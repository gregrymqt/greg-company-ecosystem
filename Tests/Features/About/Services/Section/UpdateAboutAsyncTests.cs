using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Features.About.Services.Section;

public class UpdateAboutAsyncTests : AboutServiceTestBase
{
    // --- TESTES ---

    [Theory]
    [InlineData(true)] // Testa como Chunk
    [InlineData(false)] // Testa como Upload Normal
    public async Task UpdateSection_WhenFileExists_ShouldUpdateSuccessfully(bool isChunk)
    {
        // Arrange
        var entity = CreateFakeSectionEntity(fileId: 10);
        var dto = CreateFakeAboutSectionDto(isChunk);

        _repository.Setup(r => r.GetSectionByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync("temp/path");

        _fileService
            .Setup(f =>
                f.SubstituirArquivoDoTempAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(CreateFakeEntityFile());

        _fileService
            .Setup(f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(CreateFakeEntityFile());

        // Act - IMPORTANTE: Usar await aqui!
        var result = await _sut.UpdateSectionAsync(1, dto);

        // Assert
        Assert.True(result);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSection_WhenChunkIsIntermediary_ShouldReturnFalseImmediately()
    {
        // Arrange
        _repository
            .Setup(r => r.GetSectionByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CreateFakeSectionEntity());
        _fileService
            .Setup(f =>
                f.ProcessChunkAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync((string)null!); // Simula que ainda não é o último chunk

        // Act
        var result = await _sut.UpdateSectionAsync(1, CreateFakeAboutSectionDto(isChunk: true));

        // Assert
        Assert.False(result);

        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never); // Malícia: garante que não salvou no banco
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSection_WhenSectionDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        int idInexistente = 999;
        var dto = CreateFakeAboutSectionDto(isChunk: false);

        // O repositório retorna nulo, simulando que a seção não existe no banco
        _repository
            .Setup(r => r.GetSectionByIdAsync(idInexistente))
            .ReturnsAsync((AboutSection)null!); // Null com "!" para evitar warning do compilador

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            _sut.UpdateSectionAsync(idInexistente, dto)
        );

        // Verificações de segurança: Garantia de que NADA foi feito indevidamente
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
        _fileService.Verify(
            f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
            Times.Never
        );
        _repository.Verify(r => r.UpdateSectionAsync(It.IsAny<AboutSection>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSection_WhenFileServiceThrowsException_ShouldThrowAndAbortTransaction()
    {
        // Arrange
        int idValido = 1;
        var entity = CreateFakeSectionEntity(fileId: 10); // Entidade com arquivo existente
        var dto = CreateFakeAboutSectionDto(isChunk: false); // Simula upload normal (sem ser chunk)

        _repository.Setup(r => r.GetSectionByIdAsync(idValido)).ReturnsAsync(entity);

        // Simulamos uma falha no serviço de arquivos (ex: erro de permissão na pasta, disco cheio, etc)
        _fileService
            .Setup(f => f.SubstituirArquivoAsync(It.IsAny<int>(), It.IsAny<IFormFile>()))
            .ThrowsAsync(new Exception("Falha simulada no disco ao salvar o arquivo."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _sut.UpdateSectionAsync(idValido, dto)
        );

        // Valida se a mensagem que subiu é a mesma que o serviço de arquivo estourou
        Assert.Equal("Falha simulada no disco ao salvar o arquivo.", exception.Message);

        // A malícia: O banco NÃO pode ter sido commitado se a foto falhou!
        _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
