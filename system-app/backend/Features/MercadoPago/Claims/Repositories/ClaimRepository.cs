using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Claims.Interfaces;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Repositories;

public class ClaimRepository(ApiDbContext context) : IClaimRepository
{
    public async Task<Models.Claims?> GetByIdAsync(long id)
    {
        return await context.Claims.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(List<Models.Claims> Claims, int TotalCount)> GetPaginatedClaimsAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    )
    {
        var query = context.Claims.Include(c => c.User).AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c =>
                c.MpClaimId.ToString().Contains(searchTerm) ||
                (c.User != null && !string.IsNullOrEmpty(c.User.Name) && c.User.Name.Contains(searchTerm))
            );
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(c => c.Status.ToString() == statusFilter);
        }

        var totalCount = await query.CountAsync();

        var claims = await query
            .OrderByDescending(c => c.DataCreated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (claims, totalCount);
    }

    public void UpdateClaimStatus(Models.Claims claim, InternalClaimStatus newStatus)
    {
        claim.Status = newStatus;
        context.Claims.Update(claim);
    }

    public void Update(Models.Claims claim)
    {
        context.Claims.Update(claim);
    }

    public async Task AddAsync(Models.Claims claim)
    {
        await context.Claims.AddAsync(claim);
    }

    public async Task<bool> ExistsByMpClaimIdAsync(long mpClaimId)
    {
        return await context.Claims.AsNoTracking().AnyAsync(c => c.MpClaimId == mpClaimId);
    }

    public async Task<Models.Claims?> GetByMpClaimIdAsync(long mpClaimId)
    {
        return await context.Claims.FirstOrDefaultAsync(c => c.MpClaimId == mpClaimId);
    }

    public async Task<List<Models.Claims>> GetClaimsByUserIdAsync(string userId)
    {
        return await context
            .Claims.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.DataCreated)
            .ToListAsync();
    }
}
