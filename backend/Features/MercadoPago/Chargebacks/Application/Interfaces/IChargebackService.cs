using System.Threading.Tasks;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.ViewModels;
using static MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.ViewModels.ChargeBackViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Interfaces;

public interface IChargebackService
{
    Task<ChargebacksIndexViewModel> GetChargebacksAsync(
        string? searchTerm,
        string? statusFilter,
        int page
    );

    Task<ChargebackDetailViewModel> GetChargebackDetailAsync(string chargebackId);
}



