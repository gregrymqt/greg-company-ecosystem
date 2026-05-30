using MeuCrudCsharp.Features.Emails.Interfaces;
using MeuCrudCsharp.Features.Emails.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Services;

public class SubscriptionCreateNotificationService(
    ILogger<SubscriptionCreateNotificationService> logger,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    IEmailSenderService emailService,
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

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Assinatura {ExternalId} atualizada com sucesso no banco de dados.",
                externalId
            );

            if (subscription is { User: not null, Plan: not null })
            {
                await SendSubscriptionCreatedEmailAsync(subscription);
            }
            else
            {
                logger.LogWarning(
                    "Assinatura {ExternalId} não possui User ou Plan associado. Email não será enviado.",
                    externalId
                );
            }
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

    private async Task SendSubscriptionCreatedEmailAsync(Models.Subscription subscription)
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

        await SendEmailFromTemplateAsync(
            recipientEmail: subscription.User.Email,
            subject: "Sua assinatura foi criada com sucesso!",
            viewPath: "/Pages/EmailTemplates/SubscriptionCreate/index.cshtml",
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

            await emailService.SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);

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
