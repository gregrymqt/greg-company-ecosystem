using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Interfaces;

public interface IChargebackRepository
{
    Task<(List<Chargeback> Chargebacks, int TotalCount)> GetPaginatedChargebacksAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    );
    Task<bool> ExistsByExternalIdAsync(long chargebackId);
    Task<Chargeback?> GetByExternalIdAsync(long chargebackId);
    Task AddAsync(Chargeback chargeback);
    void Update(Chargeback chargeback);
}

