using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using MeuCrudCsharp.Features.Hubs.Infrastructure.State;
namespace MeuCrudCsharp.Features.Hubs.Presentation.Hubs
{
    [Authorize]
    public class RefundProcessingHub(ConnectionMapping<string> mapping) : Hub
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

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            mapping.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
