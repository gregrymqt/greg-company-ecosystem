using System;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;

public interface IChargeBackNotificationService
{
    Task VerifyAndProcessChargeBackAsync(ChargebackNotificationPayload chargebackData);
}
