using MeuCrudCsharp.Features.Hubs;
using MeuCrudCsharp.Features.MercadoPago.Notification.Record;
using Microsoft.AspNetCore.SignalR;

namespace MeuCrudCsharp.Features.MercadoPago.Hub
{
    public class PaymentNotificationHub(
        IHubContext<PaymentProcessingHub> hubContext,
        ConnectionMapping<string> mapping)
        : IPaymentNotificationHub
    {
        public async Task SendStatusUpdateAsync(string userId, PaymentStatusUpdate update)
        {
            var connectionIds = mapping.GetConnections(userId).ToList();

            if (connectionIds.Count != 0)
            {
                await hubContext
                    .Clients.Clients(connectionIds)
                    .SendAsync("UpdatePaymentStatus", update);
            }
        }
    }
}
