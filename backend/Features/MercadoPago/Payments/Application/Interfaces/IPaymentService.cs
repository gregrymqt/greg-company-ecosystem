using System;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(string userId);
}
