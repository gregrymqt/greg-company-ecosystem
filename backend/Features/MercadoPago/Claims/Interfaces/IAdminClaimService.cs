using MeuCrudCsharp.Features.MercadoPago.Claims.ViewModels;
using static MeuCrudCsharp.Features.MercadoPago.Claims.ViewModels.MercadoPagoClaimsViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Services;

public interface IAdminClaimService
{
    Task<ClaimsIndexViewModel> GetClaimsAsync(string? searchTerm, string? statusFilter, int page);
    Task<ClaimDetailViewModel> GetClaimDetailsAsync(long localId);
    Task ReplyToClaimAsync(long localId, string messageText);
}
