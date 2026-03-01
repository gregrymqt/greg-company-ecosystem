using MeuCrudCsharp.Features.Emails.Interfaces;
using MeuCrudCsharp.Features.Emails.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Clients.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Services;

public class CardUpdateNotificationService(
    ILogger<CardUpdateNotificationService> logger,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    IClientMercadoPagoService mpService,
    IEmailSenderService emailSenderService,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    IOptions<GeneralSettings> generalSettings)
    : ICardUpdateNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task VerifyAndProcessCardUpdate(CardUpdateNotificationPayload cardUpdatePayload)
    {
        if (string.IsNullOrWhiteSpace(cardUpdatePayload.CustomerId))
        {
            logger.LogWarning("Payload com CustomerId inválido. Processo será ignorado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(cardUpdatePayload.NewCardId))
        {
            logger.LogWarning("Payload com NewCardId inválido. Processo será ignorado.");
            return;
        }

        try
        {
            logger.LogInformation(
                "Iniciando processamento de atualização de cartão para o CustomerId: {CustomerId}",
                cardUpdatePayload.CustomerId
            );

            var subscription = await subscriptionRepository.GetActiveSubscriptionByCustomerIdAsync(
                cardUpdatePayload.CustomerId
            );

            if (subscription?.User == null)
            {
                logger.LogWarning(
                    "Nenhuma assinatura ativa encontrada para o CustomerId: {CustomerId}. O processo será ignorado.",
                    cardUpdatePayload.CustomerId
                );
                return;
            }

            var cardDetails = await mpService.GetCardAsync(
                cardUpdatePayload.CustomerId,
                cardUpdatePayload.NewCardId
            );

            if (cardDetails == null || string.IsNullOrEmpty(cardDetails.LastFourDigits))
            {
                throw new AppServiceException(
                    $"Não foi possível obter os detalhes do cartão {cardUpdatePayload.NewCardId} do Mercado Pago."
                );
            }

            subscription.CardTokenId = cardDetails.Id;
            subscription.LastFourCardDigits = cardDetails.LastFourDigits;
            subscription.UpdatedAt = DateTime.UtcNow;

            subscriptionRepository.Update(subscription);

            logger.LogInformation(
                "Assinatura {SubscriptionId} marcada para atualização com o novo cartão de final {LastFourDigits}.",
                subscription.Id,
                cardDetails.LastFourDigits
            );

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Assinatura {SubscriptionId} atualizada com sucesso no banco de dados.",
                subscription.Id
            );

            await SendCardUpdateEmailAsync(subscription.User, cardDetails.LastFourDigits);

            logger.LogInformation(
                "Processamento de atualização de cartão concluído com sucesso para CustomerId: {CustomerId}",
                cardUpdatePayload.CustomerId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar atualização de cartão para CustomerId: {CustomerId}",
                cardUpdatePayload.CustomerId
            );
            throw;
        }
    }

    private async Task SendCardUpdateEmailAsync(Users user, string lastFourDigits)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            logger.LogWarning(
                "Usuário {UserId} sem email. Não foi possível enviar notificação de atualização de cartão.",
                user.Id
            );
            return;
        }

        var viewModel = new CardUpdateEmailViewModel(
            UserName: user.Name ?? "Cliente",
            LastFourDigits: lastFourDigits,
            AccountUrl: $"{_generalSettings.FrontendUrl}/perfil"
        );

        await SendEmailFromTemplateAsync(
            recipientEmail: user.Email,
            subject: "Seu método de pagamento foi atualizado",
            viewPath: "Pages/EmailTemplates/CardUpdate/index.cshtml",
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
            "Renderizando template de e-mail '{ViewPath}' para {RecipientEmail}.",
            viewPath,
            recipientEmail
        );
        var htmlBody = await razorViewToStringRenderer.RenderViewToStringAsync(viewPath, model);
        const string plainTextBody =
            $"Seu método de pagamento foi atualizado com sucesso. Para mais detalhes, acesse sua conta.";

        await emailSenderService.SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
        logger.LogInformation(
            "E-mail de atualização de cartão enviado para {RecipientEmail}.",
            recipientEmail
        );
    }
}