using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Infrastructure.Persistence.Repositories;

public class ChargebackRepository : IChargebackRepository
{
    private readonly ApplicationDbContext _context;

    public ChargebackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Chargeback> Chargebacks, int TotalCount)> GetPaginatedChargebacksAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    )
    {
        var query = _context.Chargebacks.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var userQuery = _context.Users.Where(u => u.Name != null && u.Name.ToLower().Contains(searchTerm.ToLower()));
            var userIds = await userQuery.Select(u => u.Id).ToListAsync();

            query = query.Where(c =>
                c.MercadoPagoChargebackId.Contains(searchTerm)
                || (c.UserId.HasValue && userIds.Contains(c.UserId.Value)));
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(c => c.Status == statusFilter);
        }

        var totalCount = await query.CountAsync();

        var chargebacks = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var chargebackUserIds = chargebacks.Where(c => c.UserId.HasValue).Select(c => c.UserId!.Value).Distinct().ToList();
        if (chargebackUserIds.Any())
        {
            var users = await _context.Users.Where(u => chargebackUserIds.Contains(u.Id)).ToListAsync();
            foreach (var c in chargebacks)
            {
                if (c.UserId.HasValue)
                {
                    c.User = users.FirstOrDefault(u => u.Id == c.UserId.Value);
                }
            }
        }

        return (chargebacks, totalCount);
    }

    public async Task<bool> ExistsByExternalIdAsync(string chargebackId)
    {
        return await _context.Chargebacks.AnyAsync(c => c.MercadoPagoChargebackId == chargebackId);
    }

    public async Task<Chargeback?> GetByExternalIdAsync(string chargebackId)
    {
        return await _context.Chargebacks.FirstOrDefaultAsync(c => c.MercadoPagoChargebackId == chargebackId);
    }

    public async Task AddAsync(Chargeback chargeback)
    {
        await _context.Chargebacks.AddAsync(chargeback);
        await _context.SaveChangesAsync();
    }

    public void Update(Chargeback chargeback)
    {
        _context.Chargebacks.Update(chargeback);
    }
}
