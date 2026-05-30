using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces
{
    public interface IMercadoPagoPaymentService
    {
        Task<MercadoPagoPaymentDetails?> GetPaymentStatusAsync(string externalPaymentId);
    }
}
