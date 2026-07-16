using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Plans.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Infrastructure.Persistence.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly ApplicationDbContext _context;

    public PlanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Plan plan)
    {
        await _context.Plans.AddAsync(plan);
        await _context.SaveChangesAsync();
    }

    public void Update(Plan plan)
    {
        _context.Plans.Update(plan);
    }

    public void Remove(object payload)
    {
        if (payload is Plan plan)
        {
            _context.Plans.Remove(plan);
        }
    }

    public async Task<Plan?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true)
    {
        return await _context.Plans.FirstOrDefaultAsync(p => p.PublicId == publicId);
    }

    public async Task<Plan?> GetActiveByExternalIdAsync(string externalId)
    {
        return await _context.Plans.FirstOrDefaultAsync(p => p.IsActive && p.ExternalPlanId == externalId);
    }

    public async Task<PagedResultDto<Plan>> GetActivePlansAsync(int page, int pageSize)
    {
        var query = _context.Plans.Where(p => p.IsActive);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.TransactionAmount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<Plan>(items, page, pageSize, totalCount);
    }

    public async Task<List<Plan>> GetByExternalIdsAsync(IEnumerable<string> externalIds)
    {
        return await _context.Plans
            .Where(p => externalIds.Contains(p.ExternalPlanId))
            .ToListAsync();
    }
}
