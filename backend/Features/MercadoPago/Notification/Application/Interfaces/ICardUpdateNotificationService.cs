using System;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;

public interface ICardUpdateNotificationService
{
    Task VerifyAndProcessCardUpdate(CardUpdateNotificationPayload payload);
}
