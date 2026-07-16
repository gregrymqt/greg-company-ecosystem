using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly IMongoCollection<Subscription> _subscriptions;
    private readonly IMongoCollection<Plan> _plans;
    private readonly IMongoCollection<Users> _users;

    public SubscriptionRepository(IMongoDbContext context)
    {
        _subscriptions = context.GetCollection<Subscription>("subscriptions");
        _plans = context.GetCollection<Plan>("plans");
        _users = context.GetCollection<Users>("users");
    }

    public async Task AddAsync(Subscription subscription)
    {
        await _subscriptions.InsertOneAsync(subscription);
    }

    public void Update(Subscription subscription)
    {
        _subscriptions.ReplaceOne(s => s.Id == subscription.Id, subscription);
    }

    public void Remove(Subscription subscription)
    {
        _subscriptions.DeleteOne(s => s.Id == subscription.Id);
    }

    public async Task<Subscription?> GetByExternalIdAsync(
        string externalId,
        bool includePlan = false,
        bool asNoTracking = true
    )
    {
        var subscription = await _subscriptions.Find(s => s.ExternalId == externalId).FirstOrDefaultAsync();
        
        if (subscription != null && includePlan && !string.IsNullOrEmpty(subscription.PlanId))
        {
            subscription.Plan = await _plans.Find(p => p.Id == subscription.PlanId).FirstOrDefaultAsync();
        }

        return subscription;
    }

    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(string userId)
    {
        var activeStatuses = new[] { "authorized", "pending", "paused" };

        var filter = Builders<Subscription>.Filter.And(
            Builders<Subscription>.Filter.Eq(s => s.UserId, userId),
            Builders<Subscription>.Filter.In(s => s.Status, activeStatuses)
        );

        var subscription = await _subscriptions.Find(filter)
            .SortByDescending(s => s.CurrentPeriodEndDate)
            .FirstOrDefaultAsync();

        if (subscription != null && !string.IsNullOrEmpty(subscription.PlanId))
        {
            subscription.Plan = await _plans.Find(p => p.Id == subscription.PlanId).FirstOrDefaultAsync();
        }

        return subscription;
    }

    public async Task<Subscription?> GetActiveSubscriptionByCustomerIdAsync(string customerId)
    {
        var activeStatuses = new[] { "authorized", "pending", "paused" };

        // 1. Fetch the user by customerId
        var user = await _users.Find(u => u.CustomerId == customerId).FirstOrDefaultAsync();
        
        if (user == null)
        {
            return null;
        }

        // 2. Fetch the active subscription for that user
        var filter = Builders<Subscription>.Filter.And(
            Builders<Subscription>.Filter.Eq(s => s.UserId, user.Id),
            Builders<Subscription>.Filter.In(s => s.Status, activeStatuses)
        );

        var subscription = await _subscriptions.Find(filter)
            .SortByDescending(s => s.CurrentPeriodEndDate)
            .FirstOrDefaultAsync();

        if (subscription != null)
        {
            subscription.User = user;
        }

        return subscription;
    }

    public async Task<bool> HasActiveSubscriptionByUserIdAsync(string userId)
    {
        var filter = Builders<Subscription>.Filter.And(
            Builders<Subscription>.Filter.Eq(s => s.UserId, userId),
            Builders<Subscription>.Filter.Gt(s => s.CurrentPeriodEndDate, DateTime.UtcNow),
            Builders<Subscription>.Filter.In(s => s.Status, new[] { "paid", "authorized" })
        );

        return await _subscriptions.Find(filter).AnyAsync();
    }

    public async Task<Subscription?> GetByIdAsync(string subscriptionId)
    {
        return await _subscriptions.Find(s => s.ExternalId == subscriptionId).FirstOrDefaultAsync();
    }

    public async Task<Subscription?> GetByPaymentIdAsync(
        string paymentId,
        bool includePlan = false,
        bool includeUser = false
    )
    {
        var subscription = await _subscriptions.Find(s => s.PaymentId == paymentId).FirstOrDefaultAsync();

        if (subscription != null)
        {
            if (includePlan && !string.IsNullOrEmpty(subscription.PlanId))
            {
                subscription.Plan = await _plans.Find(p => p.Id == subscription.PlanId).FirstOrDefaultAsync();
            }

            if (includeUser && !string.IsNullOrEmpty(subscription.UserId))
            {
                subscription.User = await _users.Find(u => u.Id == subscription.UserId).FirstOrDefaultAsync();
            }
        }

        return subscription;
    }

    public async Task<(IEnumerable<Subscription> Items, long TotalCount)> GetPaginatedSubscriptionsAsync(
        int page, 
        int pageSize, 
        string? statusFilter = null, 
        string? searchTerm = null
    )
    {
        var builder = Builders<Subscription>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(statusFilter))
        {
            filter &= builder.Eq(s => s.Status, statusFilter);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchFilter = builder.Or(
                builder.Regex(s => s.PayerEmail, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                builder.Regex(s => s.UserId, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            filter &= searchFilter;
        }

        var totalCount = await _subscriptions.CountDocumentsAsync(filter);

        var items = await _subscriptions.Find(filter)
            .SortByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

