using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System;

using MeuCrudCsharp.Features.Hubs.Infrastructure.State;

namespace MeuCrudCsharp.Features.Hubs.Presentation.Hubs
{
    public class GlobalRealtimeHub(ConnectionMapping<string> mapping) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                mapping.Add(userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public async Task SubscribeToJobProgress(string storageIdentifier)
        {
            var groupName = $"processing-{storageIdentifier}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            mapping.Add(storageIdentifier, Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var key = mapping.GetKey(Context.ConnectionId);

            if (key != null)
            {
                // Remove from processing group if the key was a storageIdentifier
                var groupName = $"processing-{key}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }

            mapping.Remove(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
