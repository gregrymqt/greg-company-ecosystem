using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;

using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Infrastructure.Persistence.Repositories;

public class ClaimRepository(IMongoDbContext context) : IClaimRepository
{
    private readonly IMongoCollection<Domain.Entities.Claims> _claims = context.GetCollection<Domain.Entities.Claims>("claims");
    private readonly IMongoCollection<Users> _users = context.GetCollection<Users>("users");

    public async Task<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims?> GetByIdAsync(string id)
    {
        var claim = await _claims.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (claim != null && !string.IsNullOrEmpty(claim.UserId))
        {
            claim.User = await _users.Find(u => u.Id == claim.UserId).FirstOrDefaultAsync();
        }
        return claim;
    }

    public async Task<(List<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims> Claims, int TotalCount)> GetPaginatedClaimsAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    )
    {
        var builder = Builders<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            // Busca pelo ID da claim
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.All(char.IsDigit))
            {
                filter &= builder.Eq(c => c.MercadoPagoClaimId, searchTerm);
            }
            else
            {
                var userFilter = Builders<Users>.Filter.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
                var userIds = await _users.Find(userFilter).Project(u => u.Id).ToListAsync();
                filter &= builder.In(c => c.UserId, userIds);
            }
        }

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ClaimStatus>(statusFilter, true, out var statusEnum))
        {
            filter &= builder.Eq(c => c.Status, statusEnum);
        }

        var totalCount = (int)await _claims.CountDocumentsAsync(filter);

        var claims = await _claims.Find(filter)
            .SortByDescending(c => c.DateCreated)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        var claimUserIds = claims.Where(c => !string.IsNullOrEmpty(c.UserId)).Select(c => c.UserId).Distinct().ToList();
        if (claimUserIds.Any())
        {
            var users = await _users.Find(u => claimUserIds.Contains(u.Id)).ToListAsync();
            foreach (var c in claims)
            {
                if (!string.IsNullOrEmpty(c.UserId))
                {
                    c.User = users.FirstOrDefault(u => u.Id == c.UserId);
                }
            }
        }

        return (claims, totalCount);
    }

    public void UpdateClaimStatus(MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims claim, ClaimStatus newStatus)
    {
        claim.Status = newStatus;
        _claims.ReplaceOne(c => c.Id == claim.Id, claim);
    }

    public void Update(MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims claim)
    {
        _claims.ReplaceOne(c => c.Id == claim.Id, claim);
    }

    public async Task AddAsync(MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims claim)
    {
        await _claims.InsertOneAsync(claim);
    }

    public async Task<bool> ExistsByMpClaimIdAsync(string mpClaimId)
    {
        return await _claims.Find(c => c.MercadoPagoClaimId == mpClaimId).AnyAsync();
    }

    public async Task<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims?> GetByMpClaimIdAsync(string mpClaimId)
    {
        return await _claims.Find(c => c.MercadoPagoClaimId == mpClaimId).FirstOrDefaultAsync();
    }

    public async Task<List<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims>> GetClaimsByUserIdAsync(string userId)
    {
        return await _claims.Find(c => c.UserId == userId)
            .SortByDescending(c => c.DateCreated)
            .ToListAsync();
    }
}




