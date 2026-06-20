using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

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

