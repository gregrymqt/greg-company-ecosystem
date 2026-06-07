using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MeuCrudCsharp.Features.Hubs.Presentation.Hubs
{
    public class GlobalRealtimeHub : Hub
    {
        public async Task SubscribeToJobProgress(string storageIdentifier)
        {
            var groupName = $"processing-{storageIdentifier}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
