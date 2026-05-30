namespace MeuCrudCsharp.Features.Videos.Interfaces
{
    public interface IVideoProcessingService
    {
        Task ProcessVideoToHlsAsync(int videoId, int fileId);
    }
}
