using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Plans.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Plans.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Repositories;

public class PlanRepository(ApiDbContext context) : IPlanRepository
{
    public async Task AddAsync(Plan plan) => await context.Plans.AddAsync(plan);

    public void Update(Plan plan) => context.Plans.Update(plan);

    public void Remove(object payload) => context.Remove(payload);

    public async Task<Plan?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true)
    {
        IQueryable<Plan> query = context.Plans;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(p => p.PublicId == publicId);
    }

    public async Task<Plan?> GetActiveByExternalIdAsync(string externalId) =>
        await context
            .Plans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsActive && p.ExternalPlanId == externalId);

    public async Task<PagedResultDto<Plan>> GetActivePlansAsync(int page, int pageSize)
    {
        var query = context.Plans.AsNoTracking().Where(p => p.IsActive == true);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.TransactionAmount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<Plan>(items, page, pageSize, totalCount);
    }

    public async Task<List<Plan>> GetByExternalIdsAsync(IEnumerable<string> externalIds) =>
        await context.Plans.Where(p => externalIds.Contains(p.ExternalPlanId)).ToListAsync();
}
