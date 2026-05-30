using System;
using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces
{
    public interface INotificationPayment
    {
        Task VerifyAndProcessNotificationAsync(string internalPaymentId);
    }
}
