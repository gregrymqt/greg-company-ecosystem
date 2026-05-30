using System;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;

public interface IPlanUpdateNotificationService
{
    Task VerifyAndProcessPlanUpdate(string externalId);
}
