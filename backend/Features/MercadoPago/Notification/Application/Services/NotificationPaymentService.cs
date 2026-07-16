using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using System.Text.Json;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class NotificationPaymentService(
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    IEmailSenderService emailSender,
    IRazorViewToStringRenderer razorRenderer,
    ILogger<NotificationPaymentService> logger,
    IMercadoPagoPaymentService mercadoPagoService,
    IRefundNotification refundNotification,
    ISubscriptionService subscriptionService,
    IOptions<GeneralSettings> generalSettings)
    : INotificationPayment
{

    public async Task VerifyAndProcessNotificationAsync(string internalPaymentId)
    {
        logger.LogInformation(
            "Iniciando processamento de notifica��o para PaymentId: {PaymentId}",
            internalPaymentId
        );

        try
        {
            var localPayment = await paymentRepository.GetByIdWithUserAsync(Guid.Parse(internalPaymentId));

            if (localPayment == null)
                throw new ResourceNotFoundException(
                    $"Pagamento com ID {internalPaymentId} n�o foi encontrado."
                );

            var user = localPayment.User;
            if (user == null)
                throw new ResourceNotFoundException(
                    $"Usu�rio associado ao pagamento {internalPaymentId} n�o foi encontrado."
                );

            if (localPayment.Status != "pending" && localPayment.Status != "in_process")
            {
                logger.LogInformation(
                    "Pagamento {PaymentId} j� foi processado (Status: {Status}). Ignorando notifica��o.",
                    internalPaymentId,
                    localPayment.Status
                );
                return;
            }

            if (string.IsNullOrEmpty(localPayment.ExternalId))
            {
                throw new InvalidOperationException(
                    $"Pagamento {internalPaymentId} n�o possui ExternalId."
                );
            }

            var externPayment = await mercadoPagoService.GetPaymentStatusAsync(localPayment.ExternalId);

            if (externPayment == null)
            {
                logger.LogWarning(
                    "N�o foi poss�vel obter detalhes do pagamento externo {ExternalId}",
                    localPayment.ExternalId
                );
                throw new Exception(
                    $"Falha ao obter detalhes do pagamento {localPayment.ExternalId ?? "desconhecido"} do Mercado Pago."
                );
            }

            await ProcessPaymentStatusAsync(localPayment, externPayment, user);

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Pagamento {PaymentId} processado com sucesso. Novo status: {Status}",
                internalPaymentId,
                localPayment.Status
            );

            await SendEmailBasedOnStatusAsync(localPayment.Status, user, internalPaymentId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar notifica��o para PaymentId: {PaymentId}",
                internalPaymentId
            );
            throw;
        }
    }

    private async Task ProcessPaymentStatusAsync(
        MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payment localPayment,
        dynamic externPayment,
        Users user)
    {
        switch (externPayment.Status)
        {
            case "approved":
                await ProcessApprovedPaymentAsync(localPayment, externPayment, user);
                break;

            case "rejected":
            case "cancelled":
                await ProcessRejectedPaymentAsync(localPayment, externPayment);
                break;

            case "refunded":
                await ProcessRefundedPaymentAsync(localPayment);
                break;

            default:
                logger.LogWarning(
                    "Status de pagamento n�o tratado recebido do Mercado Pago: {Status}",
                    (string)externPayment.Status
                );
                break;
        }
    }

    private async Task ProcessApprovedPaymentAsync(
        MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payment localPayment,
        dynamic externPayment,
        Users user)
    {
        if (localPayment.SubscriptionId == Guid.Empty)
        {
            logger.LogInformation(
                "Pagamento {PaymentId} aprovado. Nenhuma assinatura encontrada, criando uma nova...",
                localPayment.Id
            );

            var metadata = JsonSerializer.Deserialize<PaymentMetadata>(
                externPayment.ExternalReference
            );

            if (metadata == null || metadata.PlanPublicId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    $"Metadados (ExternalReference) inv�lidos ou ausentes no pagamento {externPayment.Id}. N�o � poss�vel criar a assinatura."
                );
            }

            if (externPayment.Payer.Email != null)
            {
                await subscriptionService.ActivateSubscriptionFromSinglePaymentAsync(
                    user.Id.ToString(),
                    metadata.PlanPublicId,
                    externPayment.Id.ToString(),
                    externPayment.Payer.Email,
                    localPayment.LastFourDigits
                );
            }

            logger.LogInformation(
                "Assinatura de pagamento �nico criada com sucesso para o usu�rio {UserId}.",
                user.Id
            );
        }
        else
        {
            logger.LogInformation(
                "Pagamento {PaymentId} aprovado. Atualizando status da assinatura existente {SubscriptionId}.",
                localPayment.Id,
                localPayment.SubscriptionId
            );

            var subscription = await subscriptionRepository.GetByIdAsync(localPayment.SubscriptionId.ToString());
            if (subscription != null)
            {
                subscription.Status = "active";
                subscriptionRepository.Update(subscription);
            }
        }

        localPayment.Status = "approved";
        paymentRepository.Update(localPayment);
    }

    private async Task ProcessRejectedPaymentAsync(MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payment localPayment, dynamic externPayment)
    {
        localPayment.Status = externPayment.Status;
        paymentRepository.Update(localPayment);

        if (localPayment.SubscriptionId != Guid.Empty)
        {
            var subscription = await subscriptionRepository.GetByIdAsync(localPayment.SubscriptionId.ToString());
            if (subscription != null)
            {
                subscription.Status = externPayment.Status;
                subscriptionRepository.Update(subscription);
            }
        }
    }

    private async Task ProcessRefundedPaymentAsync(MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payment localPayment)
    {
        localPayment.Status = "refunded";
        paymentRepository.Update(localPayment);

        if (localPayment.SubscriptionId != Guid.Empty)
        {
            var subscription = await subscriptionRepository.GetByIdAsync(localPayment.SubscriptionId.ToString());
            if (subscription != null)
            {
                subscription.Status = "refunded";
                subscriptionRepository.Update(subscription);
            }
        }

        await refundNotification.SendRefundStatusUpdate(
            localPayment.UserId.ToString(),
            "completed",
            "Seu reembolso foi processado com sucesso!"
        );
    }

    private async Task SendEmailBasedOnStatusAsync(string status, Users user, string paymentId)
    {
        switch (status)
        {
            case "approved":
                await SendConfirmationEmailAsync(user, paymentId);
                break;

            case "rejected":
            case "cancelled":
                await SendRejectionEmailAsync(user, paymentId);
                break;

            case "refunded":
                await SendRefundConfirmationEmailAsync(user, paymentId);
                break;
        }
    }

    private async Task SendPaymentEmailNotificationAsync(
        Users user,
        string paymentId,
        string subject,
        string viewPath,
        object viewModel,
        string logContext
    )
    {
        try
        {
            var htmlBody = await razorRenderer.RenderViewToStringAsync(viewPath, viewModel);
            var plainTextBody = $"Ol�, {user.Name ?? "Cliente"}! Novidades sobre seu pagamento {paymentId}.";

            if (!string.IsNullOrEmpty(user.Email))
            {
                await emailSender.SendEmailAsync(user.Email, subject, htmlBody, plainTextBody);
                logger.LogInformation(
                    "E-mail de {LogContext} enviado com sucesso para {UserEmail} referente ao pagamento {PaymentId}.",
                    logContext,
                    user.Email,
                    paymentId
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao enviar e-mail de {LogContext} para {UserEmail} (PaymentId: {PaymentId}).",
                logContext,
                user.Email,
                paymentId
            );
            throw new ExternalApiException(
                $"Falha ao renderizar ou enviar o e-mail de {logContext}.",
                ex
            );
        }
    }

    private async Task SendConfirmationEmailAsync(Users user, string paymentId)
    {
        var viewModel = new ConfirmationEmailViewModel
        {
            UserName = user.Name ?? "Cliente",
            PaymentId = paymentId,
        };
        await SendPaymentEmailNotificationAsync(
            user,
            paymentId,
            "Seu pagamento foi aprovado! ??",
            "~/Pages/EmailTemplates/Confirmation/Email.cshtml",
            viewModel,
            "Confirma��o"
        );
    }

    private async Task SendRejectionEmailAsync(Users user, string paymentId)
    {
        var viewModel = new RejectionEmailViewModel()
        {
            UserName = user.Name ?? "Cliente",
            PaymentId = paymentId,
            PaymentPageUrl = $"{generalSettings.Value.FrontendUrl}/payments/{paymentId}",
            SiteUrl = generalSettings.Value.FrontendUrl!
        };
        await SendPaymentEmailNotificationAsync(
            user,
            paymentId,
            "Aten��o: Ocorreu um problema com seu pagamento",
            "~/Pages/EmailTemplates/Rejection/Email.cshtml",
            viewModel,
            "Rejei��o"
        );
    }

    private async Task SendRefundConfirmationEmailAsync(Users user, string paymentId)
    {
        var viewModel = new ConfirmationEmailViewModel
        {
            UserName = user.Name ?? "Cliente",
            PaymentId = paymentId,
        };
        await SendPaymentEmailNotificationAsync(
            user,
            paymentId,
            "Seu Reembolso foi aprovado! ??",
            "~/Pages/EmailTemplates/Refund/Email.cshtml",
            viewModel,
            "Confirma��o de Reembolso"
        );
    }
}



