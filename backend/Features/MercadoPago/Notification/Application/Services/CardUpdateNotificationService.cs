using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class CardUpdateNotificationService(ILogger<CardUpdateNotificationService> logger) : ICardUpdateNotificationService
{
    public Task VerifyAndProcessCardUpdate(CardUpdateNotificationPayload payload)
    {
        logger.LogInformation("Processing card update for payload: {Payload}", payload);
        return Task.CompletedTask;
    }
}