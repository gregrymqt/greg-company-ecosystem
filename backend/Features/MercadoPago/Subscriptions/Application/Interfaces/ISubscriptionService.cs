using System.Security.Claims;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Clients.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<Subscription> CreateSubscriptionAsync(
            string userId,
            string planExternalId,
            string savedCardId,
            string payerEmail,
            string lastFourDigits
        );

        Task<Subscription> ActivateSubscriptionFromSinglePaymentAsync(
            string userId,
            Guid planPublicId,
            string paymentId,
            string payerEmail,
            string? lastFourCardDigits
        );

        Task<SubscriptionResponseDto> GetSubscriptionByIdAsync(string subscriptionId);

        Task<SubscriptionResponseDto> UpdateSubscriptionValueAsync(
            string subscriptionId,
            UpdateSubscriptionValueDto dto
        );

        Task<SubscriptionResponseDto> UpdateSubscriptionStatusAsync(
            string subscriptionId,
            UpdateSubscriptionStatusDto dto
        );
    }
}

