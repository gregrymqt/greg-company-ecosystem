using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace MeuCrudCsharp.Features.Files.Services;

public class FileService(
    IFileRepository repository,
    IWebHostEnvironment environment,
    IUnitOfWork unitOfWork
) : IFileService
{
    private string GerarCaminhoFisico(string featureCategoria, string nomeArquivo)
    {
        var pastaDestino = Path.Combine(environment.WebRootPath, "uploads", featureCategoria);

        if (!Directory.Exists(pastaDestino))
        {
            Directory.CreateDirectory(pastaDestino);
        }

        return Path.Combine(pastaDestino, nomeArquivo);
    }

    private static string ObterContentType(string nomeArquivo)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(nomeArquivo, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

    private EntityFile CriarEntityFile(
        string nomeArquivo,
        string nomeOriginal,
        string featureCategoria,
        long tamanhoBytes
    )
    {
        return new EntityFile
        {
            NomeArquivo = nomeArquivo,
            FeatureCategoria = featureCategoria,
            ContentType = ObterContentType(nomeOriginal),
            TamanhoBytes = tamanhoBytes,
            CaminhoRelativo = Path.Combine("uploads", featureCategoria, nomeArquivo)
                .Replace("\\", "/"),
        };
    }

    public async Task<string?> ProcessChunkAsync(
        IFormFile chunk,
        string fileName,
        int chunkIndex,
        int totalChunks
    )
    {
        var tempFolderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "temp-chunks"
        );

        if (!Directory.Exists(tempFolderPath))
            Directory.CreateDirectory(tempFolderPath);

        var tempFilePath = Path.Combine(tempFolderPath, fileName);

        await using (var stream = new FileStream(tempFilePath, FileMode.Append))
        {
            await chunk.CopyToAsync(stream);
        }

        return chunkIndex == totalChunks - 1 ? tempFilePath : null;
    }

    public async Task<EntityFile> SalvarArquivoDoTempAsync(
        string tempPath,
        string nomeOriginal,
        string categoria
    )
    {
        string? caminhoFinalFisico = null;
        try
        {
            if (!File.Exists(tempPath))
                throw new FileNotFoundException("Arquivo temporário não encontrado.", tempPath);

            var novoNome = $"{Guid.NewGuid()}_{nomeOriginal}";
            caminhoFinalFisico = GerarCaminhoFisico(categoria, novoNome);

            File.Move(tempPath, caminhoFinalFisico);

            var fileInfo = new FileInfo(caminhoFinalFisico);

            var novoArquivo = CriarEntityFile(novoNome, nomeOriginal, categoria, fileInfo.Length);
            await repository.AddAsync(novoArquivo);

            await unitOfWork.CommitAsync();

            return novoArquivo;
        }
        catch
        {
            if (caminhoFinalFisico != null && File.Exists(caminhoFinalFisico))
            {
                try
                {
                    File.Delete(caminhoFinalFisico);
                }
                catch
                {
                }
            }

            if (!File.Exists(tempPath))
                throw;
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
            }
            throw;
        }
    }

    public async Task<EntityFile> SubstituirArquivoDoTempAsync(
        int fileId,
        string tempPath,
        string nomeOriginal
    )
    {
        string? caminhoAntigoFisico = null;
        string? novoCaminhoFisico = null;
        byte[]? backupArquivoAntigo = null;

        try
        {
            var arquivoBanco = await repository.GetByIdAsync(fileId);
            if (arquivoBanco == null)
                throw new ResourceNotFoundException($"Arquivo ID {fileId} não encontrado.");

            caminhoAntigoFisico = Path.Combine(
                environment.WebRootPath,
                arquivoBanco.CaminhoRelativo
            );
            if (File.Exists(caminhoAntigoFisico))
            {
                backupArquivoAntigo = await File.ReadAllBytesAsync(caminhoAntigoFisico);
                File.Delete(caminhoAntigoFisico);
            }

            var novoNome = $"{Guid.NewGuid()}_{nomeOriginal}";
            novoCaminhoFisico = GerarCaminhoFisico(arquivoBanco.FeatureCategoria, novoNome);

            if (!File.Exists(tempPath))
            {
                throw new FileNotFoundException("Arquivo temporário sumiu antes de mover.");
            }

            File.Move(tempPath, novoCaminhoFisico);

            var fileInfo = new FileInfo(novoCaminhoFisico);

            arquivoBanco.NomeArquivo = novoNome;
            arquivoBanco.ContentType = ObterContentType(nomeOriginal);
            arquivoBanco.TamanhoBytes = fileInfo.Length;
            arquivoBanco.CaminhoRelativo = Path.Combine(
                    "uploads",
                    arquivoBanco.FeatureCategoria,
                    novoNome
                )
                .Replace("\\", "/");

            await repository.UpdateAsync(arquivoBanco);

            await unitOfWork.CommitAsync();

            return arquivoBanco;
        }
        catch
        {
            if (backupArquivoAntigo != null && caminhoAntigoFisico != null)
            {
                try
                {
                    await File.WriteAllBytesAsync(caminhoAntigoFisico, backupArquivoAntigo);
                }
                catch
                {
                }
            }

            if (novoCaminhoFisico != null && File.Exists(novoCaminhoFisico))
            {
                try
                {
                    File.Delete(novoCaminhoFisico);
                }
                catch
                {
                }
            }

            if (!File.Exists(tempPath))
                throw;
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
            }
            throw;
        }
    }

    public async Task<EntityFile> SalvarArquivoAsync(IFormFile arquivo, string featureCategoria)
    {
        string? caminhoFisico = null;
        try
        {
            if (arquivo == null || arquivo.Length == 0)
                throw new AppServiceException("Nenhum arquivo enviado.");

            var nomeArquivo = $"{Guid.NewGuid()}_{arquivo.FileName}";
            caminhoFisico = GerarCaminhoFisico(featureCategoria, nomeArquivo);

            await using (var stream = new FileStream(caminhoFisico, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            var novoArquivo = CriarEntityFile(
                nomeArquivo,
                arquivo.FileName,
                featureCategoria,
                arquivo.Length
            );
            await repository.AddAsync(novoArquivo);

            await unitOfWork.CommitAsync();

            return novoArquivo;
        }
        catch
        {
            if (caminhoFisico == null || !File.Exists(caminhoFisico))
                throw;
            try
            {
                File.Delete(caminhoFisico);
            }
            catch
            {
            }
            throw;
        }
    }

    public async Task<EntityFile> SubstituirArquivoAsync(int idArquivoAntigo, IFormFile novoArquivo)
    {
        string? caminhoAntigo = null;
        string? novoCaminho = null;
        byte[]? backupArquivoAntigo = null;

        try
        {
            var arquivoBanco = await repository.GetByIdAsync(idArquivoAntigo);
            if (arquivoBanco == null)
                throw new ResourceNotFoundException("Arquivo antigo não encontrado.");

            caminhoAntigo = Path.Combine(environment.WebRootPath, arquivoBanco.CaminhoRelativo);
            if (File.Exists(caminhoAntigo))
            {
                backupArquivoAntigo = await File.ReadAllBytesAsync(caminhoAntigo);
                File.Delete(caminhoAntigo);
            }

            var novoNome = $"{Guid.NewGuid()}_{novoArquivo.FileName}";
            novoCaminho = GerarCaminhoFisico(arquivoBanco.FeatureCategoria, novoNome);

            await using (var stream = new FileStream(novoCaminho, FileMode.Create))
            {
                await novoArquivo.CopyToAsync(stream);
            }

            arquivoBanco.NomeArquivo = novoNome;
            arquivoBanco.ContentType = novoArquivo.ContentType;
            arquivoBanco.TamanhoBytes = novoArquivo.Length;
            arquivoBanco.CaminhoRelativo = Path.Combine(
                    "uploads",
                    arquivoBanco.FeatureCategoria,
                    novoNome
                )
                .Replace("\\", "/");

            await repository.UpdateAsync(arquivoBanco);

            await unitOfWork.CommitAsync();

            return arquivoBanco;
        }
        catch
        {
            if (backupArquivoAntigo != null && caminhoAntigo != null)
            {
                try
                {
                    await File.WriteAllBytesAsync(caminhoAntigo, backupArquivoAntigo);
                }
                catch
                {
                }
            }

            if (novoCaminho == null || !File.Exists(novoCaminho))
                throw;
            try
            {
                File.Delete(novoCaminho);
            }
            catch
            {
            }
            throw;
        }
    }

    public async Task DeletarArquivoAsync(int id)
    {
        var arquivo = await repository.GetByIdAsync(id);
        if (arquivo != null)
        {
            var path = Path.Combine(environment.WebRootPath, arquivo.CaminhoRelativo);
            if (File.Exists(path))
                File.Delete(path);

            await repository.DeleteAsync(arquivo);
            await unitOfWork.CommitAsync();
        }
    }
}
