using System.Linq;
using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Notifications
{
    public class RefundNotification(
        IHubContext<GlobalRealtimeHub> hubContext
    ) : IRefundNotification
    {

        public async Task SendRefundStatusUpdate(string userId, string status, string message)
        {
            await hubContext.Clients.User(userId).SendAsync("ReceiveRefundStatus", new { status, message });
        }
    }
}
