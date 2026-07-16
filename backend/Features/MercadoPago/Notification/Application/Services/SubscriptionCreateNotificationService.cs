using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class SubscriptionCreateNotificationService(
    ILogger<SubscriptionCreateNotificationService> logger,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    ApplicationDbContext dbContext,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    IOptions<GeneralSettings> generalSettings)
    : ISubscriptionCreateNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task VerifyAndProcessSubscriptionAsync(string externalId)
    {
        logger.LogInformation("Iniciando processamento de criacao de assinatura {ExternalId}.", externalId);

        try
        {
            var subscription = await subscriptionRepository.GetByExternalIdAsync(externalId, includePlan: true, asNoTracking: false);

            if (subscription == null)
                throw new ResourceNotFoundException($"Assinatura com ExternalId {externalId} nao encontrada no banco.");

            if (subscription.Status != "pending" && subscription.Status != "in_process")
            {
                logger.LogInformation("Assinatura {ExternalId} ja foi processada (Status: {Status}). Ignorando.", externalId, subscription.Status);
                return;
            }

            subscription.Status = "active";
            subscriptionRepository.Update(subscription);

            logger.LogInformation("Assinatura {ExternalId} marcada para atualizacao com status 'active'.", externalId);

            var statusEvent = new OutboxEvent
            {
                EventType = "subscription.status.changed",
                Payload = JsonSerializer.Serialize(new { userId = subscription.UserId, status = "active", planName = subscription.Plan?.Name ?? "" })
            };
            await dbContext.OutboxEvents.AddAsync(statusEvent);

            if (subscription is { User: not null, Plan: not null })
            {
                await EnqueueSubscriptionCreatedEmailAsync(subscription);
            }
            else
            {
                logger.LogWarning("Assinatura {ExternalId} nao possui User ou Plan associado. Email nao sera enviado.", externalId);
            }

            await unitOfWork.CommitAsync();
            logger.LogInformation("Assinatura {ExternalId} atualizada com sucesso no banco de dados.", externalId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar criacao de assinatura {ExternalId}", externalId);
            throw;
        }
    }

    private async Task EnqueueSubscriptionCreatedEmailAsync(Subscription subscription)
    {
        if (string.IsNullOrEmpty(subscription.User?.Email)) return;

        var viewModel = new SubscriptionCreateEmailViewModel(
            userName: subscription.User.Name ?? "Cliente",
            planName: subscription.Plan?.Name ?? "Plano",
            subscriptionId: subscription.Id.ToString(),
            currentPeriodEndDate: subscription.CurrentPeriodEndDate,
            accountUrl: $"{_generalSettings.BaseUrl}/Profile/User/index.cshtml"
        );

        await EnqueueEmailFromTemplateAsync(subscription.User.Email, "Sua assinatura foi criada com sucesso!", "/Pages/EmailTemplates/SubscriptionCreate/index.cshtml", viewModel);
    }

    private async Task EnqueueEmailFromTemplateAsync<TModel>(string recipientEmail, string subject, string viewPath, TModel model)
    {
        var htmlBody = await razorViewToStringRenderer.RenderViewToStringAsync(viewPath, model);
        var plainTextBody = $"Assunto: {subject}. Para visualizar esta mensagem, utilize um leitor de e-mail compativel com HTML.";

        var outboxEvent = new OutboxEvent
        {
            EventType = "email.send.requested",
            Payload = JsonSerializer.Serialize(new { To = recipientEmail, Subject = subject, HtmlBody = htmlBody, PlainTextBody = plainTextBody })
        };

        await dbContext.OutboxEvents.AddAsync(outboxEvent);
    }
}
