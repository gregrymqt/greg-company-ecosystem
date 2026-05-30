using System;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;

public interface IChargeBackNotificationService
{
    Task VerifyAndProcessChargeBackAsync(ChargebackNotificationPayload chargebackData);
}
