using System.Threading.Tasks;
using MeuCrudCsharp.Features.Caching.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces
{
    public interface ICreditCardPaymentService
    {
        Task<CachedResponse> CreatePaymentOrSubscriptionAsync(
            CreditCardPaymentRequestDto request,
            string idempotencyKey
        );
    }
}
