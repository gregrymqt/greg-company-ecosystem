using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Repositories;

public class PaymentRepository(ApiDbContext context) : IPaymentRepository
{
    public async Task<bool> HasAnyPaymentByUserIdAsync(string userId)
    {
        return await context.Payments.AsNoTracking().AnyAsync(p => p.UserId == userId);
    }

    public async Task<List<Models.Payments>> GetPaymentsByUserIdAndTypeAsync(
        string userId,
        string? method = null
    )
    {
        var query = context
            .Payments.AsNoTracking()
            .Where(p => p.UserId == userId);

        if (!string.IsNullOrEmpty(method))
        {
            query = query.Where(p => p.Method == method);
        }

        return await query
            .OrderByDescending(p => p.DateApproved)
            .ToListAsync();
    }

    public async Task<Models.Payments?> GetByIdWithUserAsync(string paymentId)
    {
        return await context
            .Payments.Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Models.Payments?> GetByExternalIdWithUserAsync(string externalPaymentId)
    {
        return await context
            .Payments.Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ExternalId == externalPaymentId);
    }

    public async Task<Models.Payments?> GetByExternalIdWithSubscriptionAsync(string externalId)
    {
        return await context
            .Payments.Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.ExternalId == externalId);
    }

    public void Update(Models.Payments payment)
    {
        context.Payments.Update(payment);
    }

    public async Task AddAsync(Models.Payments payment)
    {
        await context.Payments.AddAsync(payment);
    }

    public Task Remove(Models.Payments payment)
    {
        context.Payments.Remove(payment);
        return Task.CompletedTask;
    }
}
