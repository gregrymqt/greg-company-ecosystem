using System;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.ViewModels.MercadoPagoClaimsViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;

public interface IUserClaimService
{
    Task<List<ClaimSummaryViewModel>> GetMyClaimsAsync();
    Task<ClaimDetailViewModel> GetMyClaimDetailAsync(int internalId);
    Task ReplyAsync(int internalId, string message);
    Task RequestMediationAsync(int internalId);
}
