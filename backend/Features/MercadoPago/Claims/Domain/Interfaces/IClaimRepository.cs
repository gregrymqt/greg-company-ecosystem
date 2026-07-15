using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;

public interface IClaimRepository
{
    Task<(List<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims> Claims, int TotalCount)> GetPaginatedClaimsAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    );
    
    Task<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims?> GetByIdAsync(string id);
    Task<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims?> GetByMpClaimIdAsync(string mpClaimId);
    Task<bool> ExistsByMpClaimIdAsync(string mpClaimId);
    Task<List<MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims>> GetClaimsByUserIdAsync(string userId);

    Task AddAsync(MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims claim);
    void Update(MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims claim);
    void UpdateClaimStatus(MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims claim, ClaimStatus newStatus);
}
