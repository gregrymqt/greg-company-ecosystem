namespace MeuCrudCsharp.Features.Videos.Application.Interfaces
{
    public interface IVideoProcessingService
    {
        Task ProcessVideoToHlsAsync(int videoId, int fileId);
    }
}
