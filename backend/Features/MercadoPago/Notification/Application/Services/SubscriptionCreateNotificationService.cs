using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class SubscriptionCreateNotificationService(
    ILogger<SubscriptionCreateNotificationService> logger,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    IMongoDbContext mongoContext,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    IOptions<GeneralSettings> generalSettings)
    : ISubscriptionCreateNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task VerifyAndProcessSubscriptionAsync(string externalId)
    {
        logger.LogInformation(
            "Iniciando processamento de criação de assinatura {ExternalId}.",
            externalId
        );

        try
        {
            var subscription = await subscriptionRepository.GetByExternalIdAsync(
                externalId,
                includePlan: true,
                asNoTracking: false
            );

            if (subscription == null)
            {
                throw new ResourceNotFoundException(
                    $"Assinatura com ExternalId {externalId} não encontrada no banco."
                );
            }

            if (subscription.Status != "pending" && subscription.Status != "in_process")
            {
                logger.LogInformation(
                    "Assinatura {ExternalId} já foi processada (Status: {Status}). Ignorando.",
                    externalId,
                    subscription.Status
                );
                return;
            }

            subscription.Status = "active";
            subscriptionRepository.Update(subscription);

            logger.LogInformation(
                "Assinatura {ExternalId} marcada para atualização com status 'active'.",
                externalId
            );

            var statusEvent = new OutboxEvent
            {
                EventType = "subscription.status.changed",
                Payload = JsonSerializer.Serialize(new { 
                    userId = subscription.UserId, 
                    status = "active", 
                    planName = subscription.Plan?.Name ?? "" 
                })
            };
            await mongoContext.GetCollection<OutboxEvent>("OutboxEvents").InsertOneAsync(unitOfWork.Session, statusEvent);

            if (subscription is { User: not null, Plan: not null })
            {
                await EnqueueSubscriptionCreatedEmailAsync(subscription);
            }
            else
            {
                logger.LogWarning(
                    "Assinatura {ExternalId} não possui User ou Plan associado. Email não será enviado.",
                    externalId
                );
            }

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Assinatura {ExternalId} atualizada com sucesso no banco de dados.",
                externalId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar criação de assinatura {ExternalId}",
                externalId
            );
            throw;
        }
    }

    private async Task EnqueueSubscriptionCreatedEmailAsync(Subscription subscription)
    {
        if (string.IsNullOrEmpty(subscription.User?.Email))
        {
            logger.LogWarning(
                "Assinatura {SubscriptionId} sem email do usuário. Email não será enviado.",
                subscription.Id
            );
            return;
        }

        var viewModel = new SubscriptionCreateEmailViewModel(
            userName: subscription.User.Name ?? "Cliente",
            planName: subscription.Plan?.Name ?? "Plano",
            subscriptionId: subscription.Id,
            currentPeriodEndDate: subscription.CurrentPeriodEndDate,
            accountUrl: $"{_generalSettings.BaseUrl}/Profile/User/index.cshtml"
        );

        await EnqueueEmailFromTemplateAsync(
            recipientEmail: subscription.User.Email,
            subject: "Sua assinatura foi criada com sucesso!",
            viewPath: "/Pages/EmailTemplates/SubscriptionCreate/index.cshtml",
            model: viewModel
        );
    }

    private async Task EnqueueEmailFromTemplateAsync<TModel>(
        string recipientEmail,
        string subject,
        string viewPath,
        TModel model
    )
    {
        logger.LogInformation(
            "Iniciando montagem de e-mail a partir do template '{ViewPath}' para {RecipientEmail}.",
            viewPath,
            recipientEmail
        );

        var htmlBody = await razorViewToStringRenderer.RenderViewToStringAsync(
            viewPath,
            model
        );

        var plainTextBody =
            $"Assunto: {subject}. Para visualizar esta mensagem, utilize um leitor de e-mail compatível com HTML.";

        var emailPayload = new
        {
            to = recipientEmail,
            subject = subject,
            htmlBody = htmlBody,
            plainTextBody = plainTextBody
        };

        var outboxEvent = new OutboxEvent
        {
            EventType = "email.send.requested",
            Payload = JsonSerializer.Serialize(emailPayload)
        };

        await mongoContext.GetCollection<OutboxEvent>("OutboxEvents").InsertOneAsync(unitOfWork.Session, outboxEvent);
    }
}
