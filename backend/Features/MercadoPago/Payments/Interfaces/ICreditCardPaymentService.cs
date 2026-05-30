using System.Threading.Tasks;
using MeuCrudCsharp.Features.Caching.Record;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces
{
    public interface ICreditCardPaymentService
    {
        Task<CachedResponse> CreatePaymentOrSubscriptionAsync(
            CreditCardPaymentRequestDto request,
            string idempotencyKey
        );
    }
}
