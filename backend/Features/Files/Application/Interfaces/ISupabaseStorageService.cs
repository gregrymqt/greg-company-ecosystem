using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Files.Application.Interfaces;

public interface ISupabaseStorageService
{
    Task<string> UploadRawVideoAsync(string localFilePath, string fileName, string bucketName);
    Task DeleteObjectAsync(string bucketName, string key);
    Task DeleteFolderAsync(string bucketName, string prefix);
}
