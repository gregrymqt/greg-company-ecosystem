using System;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Files.Interfaces;

public interface IFileService
{
    Task<string?> ProcessChunkAsync(
        IFormFile chunk,
        string fileName,
        int chunkIndex,
        int totalChunks
    );

    Task<EntityFile> SalvarArquivoDoTempAsync(
        string tempPath,
        string nomeOriginal,
        string categoria
    );

    Task<EntityFile> SubstituirArquivoDoTempAsync(int fileId, string tempPath, string nomeOriginal);

    Task<EntityFile> SalvarArquivoAsync(IFormFile arquivo, string featureCategoria);
    Task<EntityFile> SubstituirArquivoAsync(int idArquivoAntigo, IFormFile novoArquivo);
    Task DeletarArquivoAsync(int id);
}
