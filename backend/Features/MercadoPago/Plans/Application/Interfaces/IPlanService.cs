using MeuCrudCsharp.Features.MercadoPago.Plans.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Application.Interfaces;

public interface IPlanService
{
    Task<PagedResultDto<PlanDto>> GetActiveDbPlansAsync(int page, int pageSize);

    Task<PlanDto> GetPlanDtoByIdAsync(Guid publicId);

    Task<PlanEditDto> GetPlanEditDtoByIdAsync(Guid publicId);

    Task<PlanDto> CreatePlanAsync(CreatePlanDto createDto);

    Task<PlanDto> UpdatePlanAsync(Guid publicId, UpdatePlanDto updateDto);

    Task DeletePlanAsync(Guid publicId);

    Task<PagedResultDto<PlanDto>> GetActiveApiPlansAsync(int page, int pageSize);
}
