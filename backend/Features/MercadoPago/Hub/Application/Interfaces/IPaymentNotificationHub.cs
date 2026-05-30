using MeuCrudCsharp.Features.MercadoPago.Notification.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Hub.Application.Interfaces
{
    public interface IPaymentNotificationHub
    {
        Task SendStatusUpdateAsync(string userId, PaymentStatusUpdate update);
    }
}
