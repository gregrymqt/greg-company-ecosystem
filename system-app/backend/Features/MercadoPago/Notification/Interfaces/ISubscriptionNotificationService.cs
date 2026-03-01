namespace MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;

public interface ISubscriptionNotificationService
{
    Task ProcessRenewalAsync(string paymentId);
}
