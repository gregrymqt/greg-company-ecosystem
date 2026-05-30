namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;

public interface ISubscriptionNotificationService
{
    Task ProcessRenewalAsync(string paymentId);
}
