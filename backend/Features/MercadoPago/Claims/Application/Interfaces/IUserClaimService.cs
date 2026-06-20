using System;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.ViewModels.MercadoPagoClaimsViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;

public interface IUserClaimService
{
    Task<List<ClaimSummaryViewModel>> GetMyClaimsAsync();
    Task<ClaimDetailViewModel> GetMyClaimDetailAsync(string internalId);
    Task ReplyAsync(string internalId, string message);
    Task RequestMediationAsync(string internalId);
}

