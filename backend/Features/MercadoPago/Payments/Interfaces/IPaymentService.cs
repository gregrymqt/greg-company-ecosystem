using System;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(string userId);
}
