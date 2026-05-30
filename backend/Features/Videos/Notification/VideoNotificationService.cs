using System.Linq;
using MeuCrudCsharp.Features.Hubs;
using MeuCrudCsharp.Features.Videos.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MeuCrudCsharp.Features.Videos.Notification
{
    public class VideoNotificationService : IVideoNotificationService
    {
        private readonly IHubContext<VideoProcessingHub> _hubContext;

        private readonly ConnectionMapping<string> _mapping;

        public VideoNotificationService(
            IHubContext<VideoProcessingHub> hubContext,
            ConnectionMapping<string> mapping
        )
        {
            _hubContext = hubContext;
            _mapping = mapping;
        }

        public async Task SendProgressUpdate(
            string storageIdentifier,
            string message,
            int progress,
            bool isComplete = false,
            bool isError = false
        )
        {
            var connectionIds = _mapping.GetConnections(storageIdentifier).ToList();

            if (connectionIds.Any())
            {
                await _hubContext
                    .Clients.Clients(connectionIds)
                    .SendAsync(
                        "ProgressUpdate",
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
}
