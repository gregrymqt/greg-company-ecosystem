using Amazon.S3;
using Amazon.S3.Transfer;
using MeuCrudCsharp.Features.Files.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon;

namespace MeuCrudCsharp.Features.Files.Infrastructure.Services;

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly SupabaseSettings _settings;
    private readonly IAmazonS3 _s3Client;

    public SupabaseStorageService(IOptions<SupabaseSettings> settings)
    {
        _settings = settings.Value;

        var credentials = new BasicAWSCredentials(_settings.AccessKeyID, _settings.SecretAccessKey);
        
        var config = new AmazonS3Config
        {
            ServiceURL = _settings.EndPointS3,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<string> UploadRawVideoAsync(string localFilePath, string fileName, string bucketName)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);

        using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = fileName,
            BucketName = bucketName,
            DisablePayloadSigning = true
        };

        await fileTransferUtility.UploadAsync(uploadRequest);

        return $"{bucketName}/{fileName}";
    }

    public async Task DeleteObjectAsync(string bucketName, string key)
    {
        await _s3Client.DeleteObjectAsync(bucketName, key);
    }

    public async Task DeleteFolderAsync(string bucketName, string prefix)
    {
        var listRequest = new Amazon.S3.Model.ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = prefix
        };

        var listResponse = await _s3Client.ListObjectsV2Async(listRequest);

        foreach (var obj in listResponse.S3Objects)
        {
            await _s3Client.DeleteObjectAsync(bucketName, obj.Key);
        }
    }
}
