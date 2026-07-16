using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
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
    ApplicationDbContext dbContext,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    IOptions<GeneralSettings> generalSettings)
    : ISubscriptionNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task ProcessRenewalAsync(string paymentId)
    {
        logger.LogInformation("Iniciando processamento de renovacao para o pagamento {PaymentId}.", paymentId);

        try
        {
            var subscription = await subscriptionRepository.GetByPaymentIdAsync(paymentId, includePlan: true, includeUser: true);

            if (subscription == null)
            {
                logger.LogWarning("Nenhuma assinatura encontrada para o PaymentId: {PaymentId}.", paymentId);
                return;
            }

            if (subscription.CurrentPeriodEndDate > DateTime.UtcNow)
            {
                logger.LogInformation("Assinatura {SubscriptionId} ja renovada (Expira: {ExpirationDate}).", subscription.Id, subscription.CurrentPeriodEndDate);
                return;
            }

            if (subscription.Plan == null)
                throw new InvalidOperationException($"Assinatura {subscription.Id} sem plano associado.");

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

            logger.LogInformation("Assinatura {SubscriptionId} marcada. Nova data: {NewDate}", subscription.Id, newExpirationDate);

            var statusEvent = new OutboxEvent
            {
                EventType = "subscription.status.changed",
                Payload = JsonSerializer.Serialize(new { userId = subscription.UserId, status = "active", planName = subscription.Plan?.Name ?? "" })
            };
            await dbContext.OutboxEvents.AddAsync(statusEvent);

            if (subscription.User != null && !string.IsNullOrEmpty(subscription.User.Email))
            {
                await EnqueueRenewalEmailAsync(subscription, newExpirationDate);
            }

            await unitOfWork.CommitAsync();
            logger.LogInformation("Assinatura {SubscriptionId} renovada com sucesso.", subscription.Id);
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

        await EnqueueEmailFromTemplateAsync(subscription.User!.Email!, "Sua assinatura foi renovada com sucesso!", "Pages/EmailTemplates/Renewal/index.cshtml", viewModel);
    }

    private async Task EnqueueEmailFromTemplateAsync<TModel>(string recipientEmail, string subject, string viewPath, TModel model)
    {
        var htmlBody = await razorViewToStringRenderer.RenderViewToStringAsync(viewPath, model);
        var plainTextBody = $"Assunto: {subject}. Para visualizar esta mensagem, utilize um leitor de e-mail compativel com HTML.";

        var outboxEvent = new OutboxEvent
        {
            EventType = "email.send.requested",
            Payload = JsonSerializer.Serialize(new { to = recipientEmail, subject, htmlBody, plainTextBody })
        };

        await dbContext.OutboxEvents.AddAsync(outboxEvent);
    }
}
