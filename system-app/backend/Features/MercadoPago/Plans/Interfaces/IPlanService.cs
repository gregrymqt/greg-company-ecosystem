using MeuCrudCsharp.Features.MercadoPago.Plans.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Interfaces;

public interface IPlanService
{
    Task<PagedResultDto<PlanDto>> GetActiveDbPlansAsync(int page, int pageSize);

    Task<PlanEditDto> GetPlanEditDtoByIdAsync(Guid publicId);

    Task<PlanDto> CreatePlanAsync(CreatePlanDto createDto);

    Task<PlanDto> UpdatePlanAsync(Guid publicId, UpdatePlanDto updateDto);

    Task DeletePlanAsync(Guid publicId);

    Task<PagedResultDto<PlanDto>> GetActiveApiPlansAsync(int page, int pageSize);
}
