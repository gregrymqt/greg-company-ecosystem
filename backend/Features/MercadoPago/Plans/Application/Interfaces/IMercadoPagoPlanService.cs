using MeuCrudCsharp.Features.MercadoPago.Plans.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Application.Interfaces;

public interface IMercadoPagoPlanService
{
    Task<PlanResponseDto> CreatePlanAsync(object payload);
    Task<PlanResponseDto> UpdatePlanAsync(string externalPlanId, object payload);
    Task CancelPlanAsync(string externalPlanId);
    Task<IEnumerable<PlanResponseDto>> SearchActivePlansAsync(
        int limit,
        int offset,
        string status,
        string sortBy,
        string criteria
    );

    Task<PlanResponseDto> GetPlanByExternalIdAsync(string externalPlanId);
}

