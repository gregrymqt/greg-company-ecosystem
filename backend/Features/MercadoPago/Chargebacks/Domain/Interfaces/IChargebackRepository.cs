using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Interfaces;

public interface IChargebackRepository
{
    Task<(List<Chargeback> Chargebacks, int TotalCount)> GetPaginatedChargebacksAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    );
    Task<bool> ExistsByExternalIdAsync(string chargebackId);
    Task<Chargeback?> GetByExternalIdAsync(string chargebackId);
    Task AddAsync(Chargeback chargeback);
    void Update(Chargeback chargeback);
}
