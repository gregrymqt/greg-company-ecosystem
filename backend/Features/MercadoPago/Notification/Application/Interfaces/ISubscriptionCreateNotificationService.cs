namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;

public interface ISubscriptionCreateNotificationService
{
    Task VerifyAndProcessSubscriptionAsync(string externalId);
}
