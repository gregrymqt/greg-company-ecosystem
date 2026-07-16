using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;

public interface IClaimRepository
{
    Task<(List<Claim> Claims, int TotalCount)> GetPaginatedClaimsAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    );

    Task<Claim?> GetByIdAsync(Guid id);
    Task<Claim?> GetByMpClaimIdAsync(string mpClaimId);
    Task<bool> ExistsByMpClaimIdAsync(string mpClaimId);
    Task<List<Claim>> GetClaimsByUserIdAsync(Guid userId);

    Task AddAsync(Claim claim);
    void Update(Claim claim);
    void UpdateClaimStatus(Claim claim, ClaimStatus newStatus);
}
