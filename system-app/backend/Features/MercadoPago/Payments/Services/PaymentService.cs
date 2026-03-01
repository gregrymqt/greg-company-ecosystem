using System;
using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IHttpClientFactory httpClient,
    ILogger<PaymentService> logger)
    : MercadoPagoServiceBase(httpClient, logger), IPaymentService
{
    public async Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(string userId)
    {
        var payments = await paymentRepository.GetPaymentsByUserIdAndTypeAsync(userId);

        var historyDtos = payments
            .Select(p => new PaymentHistoryDto
            {
                Id = p.ExternalId ?? p.Id.ToString(),
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PaymentMethod = p.Method,

                Description = string.IsNullOrEmpty(p.Description)
                    ? $"Pagamento via {p.Method}"
                    : p.Description,
            })
            .ToList();

        return historyDtos;
    }
}
