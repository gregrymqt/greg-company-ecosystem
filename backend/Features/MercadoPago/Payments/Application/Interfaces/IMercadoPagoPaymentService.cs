using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces
{
    public interface IMercadoPagoPaymentService
    {
        Task<MercadoPagoPaymentDetails?> GetPaymentStatusAsync(string externalPaymentId);
    }
}
