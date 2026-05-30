using System;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Interfaces;

public interface IMercadoPagoChargebackIntegrationService
{
    Task<MercadoPagoChargebackResponse?> GetChargebackDetailsFromApiAsync(string chargebackId);
}
