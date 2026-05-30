using System.Linq;
using MeuCrudCsharp.Features.Hubs;
using MeuCrudCsharp.Features.MercadoPago.Refunds.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MeuCrudCsharp.Features.MercadoPago.Refunds.Notifications
{
    public class RefundNotification(
        IHubContext<RefundProcessingHub> hubContext,
        ConnectionMapping<string> mapping
    ) : IRefundNotification
    {

        public async Task SendRefundStatusUpdate(string userId, string status, string message)
        {
            var connectionIds = mapping.GetConnections(userId).ToList();

            if (connectionIds.Count != 0)
            {
                await hubContext
                    .Clients.Clients(connectionIds)
                    .SendAsync("ReceiveRefundStatus", new { Status = status, Message = message });
            }
        }
    }
}
