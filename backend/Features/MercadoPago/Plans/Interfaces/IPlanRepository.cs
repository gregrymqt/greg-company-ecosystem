using MeuCrudCsharp.Features.MercadoPago.Plans.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Interfaces;

public interface IPlanRepository
{
    Task AddAsync(Plan plan);
    void Update(Plan plan);
    void Remove(object payload);

    Task<Plan?> GetByPublicIdAsync(Guid publicId, bool asNoTracking);

    Task<Plan?> GetActiveByExternalIdAsync(string externalId);

    Task<List<Plan>> GetByExternalIdsAsync(IEnumerable<string> externalIds);

    Task<PagedResultDto<Plan>> GetActivePlansAsync(int page, int pageSize);
}