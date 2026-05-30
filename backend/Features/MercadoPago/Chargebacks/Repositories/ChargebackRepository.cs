using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Repositories;

public class ChargebackRepository(ApiDbContext context) : IChargebackRepository
{
    public async Task<(List<Chargeback> Chargebacks, int TotalCount)> GetPaginatedChargebacksAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    )
    {
        var query = context.Chargebacks.Include(c => c.User).AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (long.TryParse(searchTerm, out var idSearch))
            {
                query = query.Where(c =>
                    c.ChargebackId == idSearch || 
                    (c.User != null && c.User.Name.Contains(searchTerm))
                );
            }
            else
            {
                query = query.Where(c => c.User != null && c.User.Name.Contains(searchTerm));
            }
        }

        if (
            !string.IsNullOrEmpty(statusFilter)
            && Enum.TryParse<ChargebackStatus>(statusFilter, true, out var statusEnum)
        )
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        // Contagem antes da paginação
        var totalCount = await query.CountAsync();

        // Paginação
        var chargebacks = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (chargebacks, totalCount);
    }

    public async Task<bool> ExistsByExternalIdAsync(long chargebackId)
    {
        return await context
            .Chargebacks.AsNoTracking()
            .AnyAsync(c => c.ChargebackId == chargebackId);
    }

    public async Task<Chargeback?> GetByExternalIdAsync(long chargebackId)
    {
        return await context.Chargebacks.FirstOrDefaultAsync(c => c.ChargebackId == chargebackId);
    }

    public async Task AddAsync(Chargeback chargeback)
    {
        await context.Chargebacks.AddAsync(chargeback);
    }

    public void Update(Chargeback chargeback)
    {
        context.Chargebacks.Update(chargeback);
    }
}
