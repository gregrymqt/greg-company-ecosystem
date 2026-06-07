using System.Linq;
using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using MeuCrudCsharp.Features.Hubs.Infrastructure.State;
using MeuCrudCsharp.Features.Videos.Application.Interfaces;

using Microsoft.AspNetCore.SignalR;

namespace MeuCrudCsharp.Features.Videos.Application.Notification
{
    public class VideoNotificationService : IVideoNotificationService
    {
        private readonly IHubContext<GlobalRealtimeHub> _hubContext;

        public VideoNotificationService(
            IHubContext<GlobalRealtimeHub> hubContext
        )
        {
            _hubContext = hubContext;
        }

        public async Task SendProgressUpdate(
            string storageIdentifier,
            string message,
            int progress,
            bool isComplete = false,
            bool isError = false
        )
        {
            await _hubContext.Clients.Group($"processing-{storageIdentifier}")
                .SendAsync(
                    "ReceiveVideoProgress",
                    new
                    {
                        Message = message,
                        Progress = progress,
                        IsComplete = isComplete,
                        IsError = isError,
                    }
                );
        }
    }
}
