using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription);
    void Update(Subscription subscription);
    void Remove(Subscription subscription);

    Task<Subscription?> GetByExternalIdAsync(
        string externalId,
        bool includePlan = false,
        bool asNoTracking = true
    );

    Task<Subscription?> GetByIdAsync(string subscriptionId);
    Task<Subscription?> GetByPaymentIdAsync(
        string paymentId,
        bool includePlan = false,
        bool includeUser = false
    );

    Task<Subscription?> GetActiveSubscriptionByUserIdAsync(string userId);
    Task<Subscription?> GetActiveSubscriptionByCustomerIdAsync(string customerId);

    Task<bool> HasActiveSubscriptionByUserIdAsync(string userId);
}

