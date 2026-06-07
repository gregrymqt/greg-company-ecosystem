using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using MeuCrudCsharp.Features.Hubs.Infrastructure.State;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using MeuCrudCsharp.Features.MercadoPago.Hub.Application.Interfaces;

namespace MeuCrudCsharp.Features.MercadoPago.Hub.Infrastructure.Services
{
    public class PaymentNotificationHub(
        IHubContext<GlobalRealtimeHub> hubContext)
        : IPaymentNotificationHub
    {
        public async Task SendStatusUpdateAsync(string userId, PaymentStatusUpdate update)
        {
            await hubContext.Clients.User(userId).SendAsync("UpdatePaymentStatus", update);
        }
    }
}
