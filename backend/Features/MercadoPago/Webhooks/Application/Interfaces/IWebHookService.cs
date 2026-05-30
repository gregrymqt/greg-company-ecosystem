using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.Interfaces
{
    public interface IWebhookService
    {
        bool IsSignatureValid(
            HttpRequest request,
            MercadoPagoWebhookNotification notification
        );

        Task ProcessWebhookNotificationAsync(
            MercadoPagoWebhookNotification notification
        );
    }
}
