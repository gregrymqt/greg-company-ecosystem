using MeuCrudCsharp.Features.Caching.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;

public interface IPixPaymentService
{
    Task<CachedResponse> CreateIdempotentPixPaymentAsync(
        CreatePixPaymentRequest request,
        string idempotencyKey
    );
}
