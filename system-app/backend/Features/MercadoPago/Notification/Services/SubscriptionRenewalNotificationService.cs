using MeuCrudCsharp.Features.Emails.Interfaces;
using MeuCrudCsharp.Features.Emails.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Services;

public class SubscriptionRenewalNotificationService(
    ILogger<SubscriptionRenewalNotificationService> logger,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    IEmailSenderService emailSenderService,
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

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Assinatura {SubscriptionId} renovada com sucesso.",
                subscription.Id
            );

            if (subscription.User != null && !string.IsNullOrEmpty(subscription.User.Email))
            {
                await SendRenewalEmailAsync(subscription, newExpirationDate);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao renovar PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }

    private async Task SendRenewalEmailAsync(Subscription subscription, DateTime newExpirationDate)
    {
        var viewModel = new RenewalEmailViewModel(
            userName: subscription.User?.Name ?? "Cliente",
            planName: subscription.Plan?.Name ?? "Plano",
            newExpirationDate: newExpirationDate,
            transactionAmount: subscription.Plan?.TransactionAmount ?? 0,
            accountUrl: $"{_generalSettings.BaseUrl}/Profile/User/index.cshtml",
            supportUrl: $"{_generalSettings.BaseUrl}/Support/Contact/index.cshtml"
        );

        await SendEmailFromTemplateAsync(
            recipientEmail: subscription.User!.Email!,
            subject: "Sua assinatura foi renovada com sucesso!",
            viewPath: "Pages/EmailTemplates/Renewal/index.cshtml",
            model: viewModel
        );
    }

    private async Task SendEmailFromTemplateAsync<TModel>(
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

        try
        {
            var htmlBody = await razorViewToStringRenderer.RenderViewToStringAsync(
                viewPath,
                model
            );

            var plainTextBody =
                $"Assunto: {subject}. Para visualizar esta mensagem, utilize um leitor de e-mail compatível com HTML.";

            await emailSenderService.SendEmailAsync(
                recipientEmail,
                subject,
                htmlBody,
                plainTextBody
            );

            logger.LogInformation(
                "E-mail para {RecipientEmail} enviado com sucesso.",
                recipientEmail
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha no processo de montagem e envio de e-mail para {RecipientEmail}.",
                recipientEmail
            );
            throw;
        }
    }
}
