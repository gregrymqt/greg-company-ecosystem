using System;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;

namespace MeuCrudCsharp.Features.Files.Application.Interfaces;

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

    Task<EntityFile> SubstituirArquivoDoTempAsync(string fileId, string tempPath, string nomeOriginal);

    Task<EntityFile> SalvarArquivoAsync(IFormFile arquivo, string featureCategoria);
    Task<EntityFile> SubstituirArquivoAsync(string idArquivoAntigo, IFormFile novoArquivo);
    Task DeletarArquivoAsync(string id);
}


