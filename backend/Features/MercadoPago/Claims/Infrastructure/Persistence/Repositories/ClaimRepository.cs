using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Infrastructure.Persistence.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly ApplicationDbContext _context;

    public ClaimRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Claim?> GetByIdAsync(Guid id)
    {
        return await _context.Claims
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(List<Claim> Claims, int TotalCount)> GetPaginatedClaimsAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    )
    {
        var query = _context.Claims.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (searchTerm.All(char.IsDigit))
            {
                query = query.Where(c => c.MercadoPagoClaimId == searchTerm);
            }
            else
            {
                var userIds = await _context.Users
                    .Where(u => u.Name != null && u.Name.ToLower().Contains(searchTerm.ToLower()))
                    .Select(u => u.Id)
                    .ToListAsync();
                query = query.Where(c => c.UserId.HasValue && userIds.Contains(c.UserId.Value));
            }
        }

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ClaimStatus>(statusFilter, true, out var statusEnum))
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        var totalCount = await query.CountAsync();

        var claims = await query
            .OrderByDescending(c => c.DateCreated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var claimUserIds = claims.Where(c => c.UserId.HasValue).Select(c => c.UserId!.Value).Distinct().ToList();
        if (claimUserIds.Any())
        {
            var users = await _context.Users.Where(u => claimUserIds.Contains(u.Id)).ToListAsync();
            foreach (var c in claims)
            {
                if (c.UserId.HasValue)
                {
                    c.User = users.FirstOrDefault(u => u.Id == c.UserId.Value);
                }
            }
        }

        return (claims, totalCount);
    }

    public void UpdateClaimStatus(Claim claim, ClaimStatus newStatus)
    {
        claim.Status = newStatus;
        _context.Claims.Update(claim);
    }

    public void Update(Claim claim)
    {
        _context.Claims.Update(claim);
    }

    public async Task AddAsync(Claim claim)
    {
        await _context.Claims.AddAsync(claim);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByMpClaimIdAsync(string mpClaimId)
    {
        return await _context.Claims.AnyAsync(c => c.MercadoPagoClaimId == mpClaimId);
    }

    public async Task<Claim?> GetByMpClaimIdAsync(string mpClaimId)
    {
        return await _context.Claims.FirstOrDefaultAsync(c => c.MercadoPagoClaimId == mpClaimId);
    }

    public async Task<List<Claim>> GetClaimsByUserIdAsync(Guid userId)
    {
        return await _context.Claims
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.DateCreated)
            .ToListAsync();
    }
}
