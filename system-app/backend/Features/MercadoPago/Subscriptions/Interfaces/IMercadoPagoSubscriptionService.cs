using MeuCrudCsharp.Features.MercadoPago.Subscriptions.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;

public interface IMercadoPagoSubscriptionService
{
    Task<SubscriptionResponseDto> CreateSubscriptionAsync(CreateSubscriptionDto payload);
    Task<SubscriptionResponseDto?> GetSubscriptionByIdAsync(string subscriptionId);
    Task UpdateSubscriptionCardAsync(string subscriptionId, string newCardToken);
    Task<SubscriptionResponseDto> UpdateSubscriptionValueAsync(string subscriptionId, UpdateSubscriptionValueDto dto);
    Task<SubscriptionResponseDto> UpdateSubscriptionStatusAsync(string subscriptionId, UpdateSubscriptionStatusDto dto);
    Task CancelSubscriptionAsync(string subscriptionId);
}