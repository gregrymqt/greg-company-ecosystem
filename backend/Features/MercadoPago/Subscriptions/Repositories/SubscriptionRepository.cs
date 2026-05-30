using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Repositories;

public class SubscriptionRepository(ApiDbContext context) : ISubscriptionRepository
{
    public async Task AddAsync(Subscription subscription)
    {
        await context.Subscriptions.AddAsync(subscription);
    }

    public void Update(Subscription subscription)
    {
        context.Subscriptions.Update(subscription);
    }

    public void Remove(Subscription subscription)
    {
        context.Subscriptions.Remove(subscription);
    }

    public async Task<Subscription?> GetByExternalIdAsync(
        string externalId,
        bool includePlan = false,
        bool asNoTracking = true
    )
    {
        IQueryable<Subscription> query = context.Subscriptions;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (includePlan)
        {
            query = query.Include(s => s.Plan);
        }

        return await query.FirstOrDefaultAsync(s => s.ExternalId == externalId);
    }

    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(string userId)
    {
        var activeStatuses = new[] { "authorized", "pending", "paused" };

        return await context
            .Subscriptions.AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && activeStatuses.Contains(s.Status))
            .OrderByDescending(s => s.CurrentPeriodEndDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription?> GetActiveSubscriptionByCustomerIdAsync(string customerId)
    {
        var activeStatuses = new[] { "authorized", "pending", "paused" };

        return await context
            .Subscriptions
            .Include(s => s.User)
            .Where(s => 
                s.User != null && 
                s.User.CustomerId == customerId && 
                activeStatuses.Contains(s.Status))
            .OrderByDescending(s => s.CurrentPeriodEndDate)
            .FirstOrDefaultAsync();
    }

    public Task<bool> HasActiveSubscriptionByUserIdAsync(string userId)
    {
        return context
            .Subscriptions.AsNoTracking()
            .AnyAsync(s =>
                s.UserId == userId
                && s.CurrentPeriodEndDate > DateTime.UtcNow
                && (s.Status == "paid" || s.Status == "authorized")
            );
    }

    public async Task<Subscription?> GetByIdAsync(string subscriptionId)
    {
        return await context.Subscriptions.FirstOrDefaultAsync(s =>
            s.ExternalId == subscriptionId
        );
    }

    public async Task<Subscription?> GetByPaymentIdAsync(
        string paymentId,
        bool includePlan = false,
        bool includeUser = false
    )
    {
        IQueryable<Subscription> query = context.Subscriptions;

        if (includePlan)
        {
            query = query.Include(s => s.Plan);
        }

        if (includeUser)
        {
            query = query.Include(s => s.User);
        }

        return await query.FirstOrDefaultAsync(s => s.PaymentId == paymentId);
    }
}
