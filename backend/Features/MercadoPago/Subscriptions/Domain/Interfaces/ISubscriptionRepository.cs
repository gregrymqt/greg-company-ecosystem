using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;

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

    Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId);
    Task<Subscription?> GetActiveSubscriptionByCustomerIdAsync(string customerId);

    Task<bool> HasActiveSubscriptionByUserIdAsync(Guid userId);

    Task<(IEnumerable<Subscription> Items, long TotalCount)> GetPaginatedSubscriptionsAsync(
        int page,
        int pageSize,
        string? statusFilter = null,
        string? searchTerm = null
    );
}
