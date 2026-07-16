using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasAnyPaymentByUserIdAsync(Guid userId)
    {
        return await _context.Payments.AnyAsync(p => p.UserId == userId);
    }

    public async Task<List<Payment>> GetPaymentsByUserIdAndTypeAsync(
        Guid userId,
        string? method = null
    )
    {
        var query = _context.Payments.Where(p => p.UserId == userId);

        if (!string.IsNullOrEmpty(method))
        {
            query = query.Where(p => p.Method == method);
        }

        return await query.OrderByDescending(p => p.DateApproved).ToListAsync();
    }

    public async Task<Payment?> GetByIdWithUserAsync(Guid paymentId)
    {
        return await _context.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> GetByExternalIdWithUserAsync(string externalPaymentId)
    {
        return await _context.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ExternalId == externalPaymentId);
    }

    public async Task<Payment?> GetByExternalIdWithSubscriptionAsync(string externalId)
    {
        return await _context.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.ExternalId == externalId);
    }

    public void Update(Payment payment)
    {
        _context.Payments.Update(payment);
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public Task Remove(Payment payment)
    {
        _context.Payments.Remove(payment);
        return Task.CompletedTask;
    }

    public async Task<(List<Payment> Items, long TotalCount)> GetAdminPaymentsPaginatedAsync(int page, int pageSize, string? status, string? search)
    {
        var query = _context.Payments.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.PayerEmail != null && p.PayerEmail.ToLower().Contains(search.ToLower()));
        }

        var totalCount = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
