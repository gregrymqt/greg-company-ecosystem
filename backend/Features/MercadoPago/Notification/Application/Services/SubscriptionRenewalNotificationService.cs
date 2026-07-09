using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;

using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class SubscriptionRenewalNotificationService(
    ILogger<SubscriptionRenewalNotificationService> logger,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    IMongoDbContext mongoContext,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    IOptions<GeneralSettings> generalSettings)
    : ISubscriptionNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task ProcessRenewalAsync(string paymentId)
    {
        logger.LogInformation(
            "Iniciando processamento de renovação para o pagamento {PaymentId}.",
            paymentId
        );

        try
        {
            var subscription = await subscriptionRepository.GetByPaymentIdAsync(
                paymentId,
                includePlan: true,
                includeUser: true
            );

            if (subscription == null)
            {
                logger.LogWarning(
                    "Nenhuma assinatura encontrada para o PaymentId: {PaymentId}.",
                    paymentId
                );
                return;
            }

            if (subscription.CurrentPeriodEndDate > DateTime.UtcNow)
            {
                logger.LogInformation(
                    "Assinatura {SubscriptionId} já renovada (Expira: {ExpirationDate}).",
                    subscription.Id,
                    subscription.CurrentPeriodEndDate
                );
                return;
            }

            if (subscription.Plan == null)
            {
                throw new InvalidOperationException(
                    $"Assinatura {subscription.Id} sem plano associado."
                );
            }

            var planInterval = subscription.Plan.FrequencyInterval;
            var planFrequency = subscription.Plan.FrequencyType;

            var newExpirationDate = subscription.CurrentPeriodEndDate;
            newExpirationDate = planFrequency switch
            {
                PlanFrequencyType.Months => newExpirationDate.AddMonths(planInterval),
                PlanFrequencyType.Days => newExpirationDate.AddDays(planInterval),
                _ => newExpirationDate
            };

            subscription.CurrentPeriodEndDate = newExpirationDate;
            subscription.Status = "active";
            subscriptionRepository.Update(subscription);

            logger.LogInformation(
                "Assinatura {SubscriptionId} marcada. Nova data: {NewDate}",
                subscription.Id,
                newExpirationDate
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

            if (subscription.User != null && !string.IsNullOrEmpty(subscription.User.Email))
            {
                await EnqueueRenewalEmailAsync(subscription, newExpirationDate);
            }

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Assinatura {SubscriptionId} renovada com sucesso.",
                subscription.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao renovar PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }

    private async Task EnqueueRenewalEmailAsync(Subscription subscription, DateTime newExpirationDate)
    {
        var viewModel = new RenewalEmailViewModel(
            userName: subscription.User?.Name ?? "Cliente",
            planName: subscription.Plan?.Name ?? "Plano",
            newExpirationDate: newExpirationDate,
            transactionAmount: subscription.Plan?.TransactionAmount ?? 0,
            accountUrl: $"{_generalSettings.BaseUrl}/Profile/User/index.cshtml",
            supportUrl: $"{_generalSettings.BaseUrl}/Support/Contact/index.cshtml"
        );

        await EnqueueEmailFromTemplateAsync(
            recipientEmail: subscription.User!.Email!,
            subject: "Sua assinatura foi renovada com sucesso!",
            viewPath: "Pages/EmailTemplates/Renewal/index.cshtml",
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
