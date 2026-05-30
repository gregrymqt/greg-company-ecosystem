namespace MeuCrudCsharp.Features.Videos.Application.Interfaces;

public interface IVideoNotificationService
{
    Task SendProgressUpdate(
        string groupName,
        string message,
        int progress,
        bool isComplete = false,
        bool isError = false
    );
}