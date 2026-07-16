using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Subscription subscription)
    {
        await _context.Subscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }

    public void Update(Subscription subscription)
    {
        _context.Subscriptions.Update(subscription);
    }

    public void Remove(Subscription subscription)
    {
        _context.Subscriptions.Remove(subscription);
    }

    public async Task<Subscription?> GetByExternalIdAsync(
        string externalId,
        bool includePlan = false,
        bool asNoTracking = true
    )
    {
        var query = _context.Subscriptions.AsQueryable();
        if (includePlan) query = query.Include(s => s.Plan);

        return await query.FirstOrDefaultAsync(s => s.ExternalId == externalId);
    }

    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId)
    {
        var activeStatuses = new[] { "authorized", "pending", "paused" };

        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && activeStatuses.Contains(s.Status))
            .OrderByDescending(s => s.CurrentPeriodEndDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription?> GetActiveSubscriptionByCustomerIdAsync(string customerId)
    {
        var activeStatuses = new[] { "authorized", "pending", "paused" };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.CustomerId == customerId);
        if (user == null) return null;

        var subscription = await _context.Subscriptions
            .Where(s => s.UserId == user.Id && activeStatuses.Contains(s.Status))
            .OrderByDescending(s => s.CurrentPeriodEndDate)
            .FirstOrDefaultAsync();

        if (subscription != null)
        {
            subscription.User = user;
        }

        return subscription;
    }

    public async Task<bool> HasActiveSubscriptionByUserIdAsync(Guid userId)
    {
        var activeStatuses = new[] { "paid", "authorized" };

        return await _context.Subscriptions.AnyAsync(s =>
            s.UserId == userId
            && s.CurrentPeriodEndDate > DateTime.UtcNow
            && activeStatuses.Contains(s.Status));
    }

    public async Task<Subscription?> GetByIdAsync(string subscriptionId)
    {
        return await _context.Subscriptions.FirstOrDefaultAsync(s => s.ExternalId == subscriptionId);
    }

    public async Task<Subscription?> GetByPaymentIdAsync(
        string paymentId,
        bool includePlan = false,
        bool includeUser = false
    )
    {
        var query = _context.Subscriptions.AsQueryable();
        if (includePlan) query = query.Include(s => s.Plan);
        if (includeUser) query = query.Include(s => s.User);

        return await query.FirstOrDefaultAsync(s => s.PaymentId == paymentId);
    }

    public async Task<(IEnumerable<Subscription> Items, long TotalCount)> GetPaginatedSubscriptionsAsync(
        int page,
        int pageSize,
        string? statusFilter = null,
        string? searchTerm = null
    )
    {
        var query = _context.Subscriptions.AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(s => s.Status == statusFilter);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(s =>
                (s.PayerEmail != null && s.PayerEmail.ToLower().Contains(searchTerm.ToLower()))
                || s.UserId.ToString().Contains(searchTerm));
        }

        var totalCount = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
