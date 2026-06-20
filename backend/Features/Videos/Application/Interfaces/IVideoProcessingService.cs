namespace MeuCrudCsharp.Features.Videos.Application.Interfaces
{
    public interface IVideoProcessingService
    {
        Task ProcessVideoToHlsAsync(string videoId, string fileId);
    }
}

