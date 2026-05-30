using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using MeuCrudCsharp.Features.Hubs.Infrastructure.State;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using MeuCrudCsharp.Features.MercadoPago.Hub.Application.Interfaces;

namespace MeuCrudCsharp.Features.MercadoPago.Hub.Infrastructure.Services
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
