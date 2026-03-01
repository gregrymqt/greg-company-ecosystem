using System.Text.Json;
using MeuCrudCsharp.Features.Emails.Interfaces;
using MeuCrudCsharp.Features.Emails.ViewModels;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Record;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Refunds.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Services;

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
            "Iniciando processamento de notificação para PaymentId: {PaymentId}",
            internalPaymentId
        );

        try
        {
            var localPayment = await paymentRepository.GetByIdWithUserAsync(internalPaymentId);
            
            if (localPayment == null)
                throw new ResourceNotFoundException(
                    $"Pagamento com ID {internalPaymentId} não foi encontrado."
                );

            var user = localPayment.User;
            if (user == null)
                throw new ResourceNotFoundException(
                    $"Usuário associado ao pagamento {internalPaymentId} não foi encontrado."
                );

            if (localPayment.Status != "pending" && localPayment.Status != "in_process")
            {
                logger.LogInformation(
                    "Pagamento {PaymentId} já foi processado (Status: {Status}). Ignorando notificação.",
                    internalPaymentId,
                    localPayment.Status
                );
                return;
            }

            if (string.IsNullOrEmpty(localPayment.ExternalId))
            {
                throw new InvalidOperationException(
                    $"Pagamento {internalPaymentId} não possui ExternalId."
                );
            }

            var externPayment = await mercadoPagoService.GetPaymentStatusAsync(localPayment.ExternalId);
            
            if (externPayment == null)
            {
                logger.LogWarning(
                    "Não foi possível obter detalhes do pagamento externo {ExternalId}",
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
                "Erro ao processar notificação para PaymentId: {PaymentId}",
                internalPaymentId
            );
            throw;
        }
    }

    private async Task ProcessPaymentStatusAsync(
        Models.Payments localPayment,
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
                    "Status de pagamento não tratado recebido do Mercado Pago: {Status}",
                    (string)externPayment.Status
                );
                break;
        }
    }

    private async Task ProcessApprovedPaymentAsync(
        Models.Payments localPayment,
        dynamic externPayment,
        Users user)
    {
        if (string.IsNullOrEmpty(localPayment.SubscriptionId))
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
                    $"Metadados (ExternalReference) inválidos ou ausentes no pagamento {externPayment.Id}. Não é possível criar a assinatura."
                );
            }

            if (externPayment.Payer.Email != null)
            {
                await subscriptionService.ActivateSubscriptionFromSinglePaymentAsync(
                    user.Id,
                    metadata.PlanPublicId,
                    externPayment.Id.ToString(),
                    externPayment.Payer.Email,
                    localPayment.LastFourDigits
                );
            }

            logger.LogInformation(
                "Assinatura de pagamento único criada com sucesso para o usuário {UserId}.",
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

            var subscription = await subscriptionRepository.GetByIdAsync(localPayment.SubscriptionId);
            if (subscription != null)
            {
                subscription.Status = "active";
                subscriptionRepository.Update(subscription);
            }
        }

        localPayment.Status = "approved";
        paymentRepository.Update(localPayment);
    }

    private async Task ProcessRejectedPaymentAsync(Models.Payments localPayment, dynamic externPayment)
    {
        localPayment.Status = externPayment.Status;
        paymentRepository.Update(localPayment);

        if (!string.IsNullOrEmpty(localPayment.SubscriptionId))
        {
            var subscription = await subscriptionRepository.GetByIdAsync(localPayment.SubscriptionId);
            if (subscription != null)
            {
                subscription.Status = externPayment.Status;
                subscriptionRepository.Update(subscription);
            }
        }
    }

    private async Task ProcessRefundedPaymentAsync(Models.Payments localPayment)
    {
        localPayment.Status = "refunded";
        paymentRepository.Update(localPayment);

        if (!string.IsNullOrEmpty(localPayment.SubscriptionId))
        {
            var subscription = await subscriptionRepository.GetByIdAsync(localPayment.SubscriptionId);
            if (subscription != null)
            {
                subscription.Status = "refunded";
                subscriptionRepository.Update(subscription);
            }
        }

        await refundNotification.SendRefundStatusUpdate(
            localPayment.UserId,
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
            var plainTextBody = $"Olá, {user.Name ?? "Cliente"}! Novidades sobre seu pagamento {paymentId}.";

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
            "Seu pagamento foi aprovado! 🎉",
            "~/Pages/EmailTemplates/Confirmation/Email.cshtml",
            viewModel,
            "Confirmação"
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
            "Atenção: Ocorreu um problema com seu pagamento",
            "~/Pages/EmailTemplates/Rejection/Email.cshtml",
            viewModel,
            "Rejeição"
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
            "Seu Reembolso foi aprovado! 🎉",
            "~/Pages/EmailTemplates/Refund/Email.cshtml",
            viewModel,
            "Confirmação de Reembolso"
        );
    }
}
