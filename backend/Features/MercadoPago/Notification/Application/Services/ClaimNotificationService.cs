using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;

using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class ClaimNotificationService(
    IClaimRepository claimRepository,
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    ILogger<ClaimNotificationService> logger,
    IMongoDbContext mongoContext,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    IOptions<GeneralSettings> generalSettings,
    IMercadoPagoIntegrationService mpIntegrationService)
    : IClaimNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task VerifyAndProcessClaimAsync(ClaimNotificationPayload claimPayload)
    {
        try
        {
            logger.LogInformation("Iniciando processamento da Claim ID: {ClaimId}", claimPayload.Id);

            if (string.IsNullOrWhiteSpace(claimPayload.Id))
            {
                logger.LogWarning("Id da claim inválido.");
                return;
            }

            if (!long.TryParse(claimPayload.Id, out var mpClaimId))
            {
                logger.LogError("ID da Claim não é um número válido: {Id}", claimPayload.Id);
                return;
            }

            var claimDetails = await mpIntegrationService.GetClaimByIdAsync(mpClaimId.ToString());

            if (claimDetails == null)
            {
                logger.LogError("Não foi possível obter detalhes da Claim {Id} na API do MP.", mpClaimId);
                return;
            }

            var resourceId = claimDetails.ResourceId;
            var resourceTypeEnum = claimDetails.Resource;

            logger.LogInformation("Claim vinculada ao Recurso: {ResourceId}, Tipo: {Type}", resourceId, resourceTypeEnum);

            Users? user;

            if (resourceTypeEnum == ClaimResource.Payment)
            {
                var payment = await paymentRepository.GetByExternalIdWithUserAsync(resourceId);
                user = payment?.User;
            }
            else
            {
                var subscription = await subscriptionRepository.GetByIdAsync(resourceId);
                user = subscription?.User;
            }

            if (user == null)
            {
                logger.LogWarning("Nenhum usuário encontrado para o Recurso ID '{ResourceId}'.", resourceId);
            }

            var existingClaim = await claimRepository.GetByMpClaimIdAsync(mpClaimId.ToString());

            if (existingClaim == null)
            {
                var newClaimRecord = new MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities.Claims
                {
                    MercadoPagoClaimId = mpClaimId.ToString(),
                    PaymentId = resourceId,
                    Type = claimDetails.Type,
                    ResourceType = resourceTypeEnum,
                    UserId = user?.Id.ToString(),
                    DateCreated = DateTime.UtcNow,
                    Status = ClaimStatus.Opened,
                    CurrentStage = claimDetails.Stage
                };

                await claimRepository.AddAsync(newClaimRecord);
                logger.LogInformation("Nova Claim ID {ClaimId} marcada para inserção.", mpClaimId);
            }
            else
            {
                logger.LogInformation("Claim ID {ClaimId} já existe. Verificando atualizações.", mpClaimId);
            }

            if (user != null && existingClaim == null)
            {
                await EnqueueClaimReceivedEmailAsync(user, mpClaimId);
            }

            await unitOfWork.CommitAsync();

            logger.LogInformation("Claim {Id} processada com sucesso.", mpClaimId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar Claim {Id}", claimPayload.Id);
            throw;
        }
    }

    private async Task EnqueueClaimReceivedEmailAsync(Users user, long claimId)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            logger.LogWarning("Email do usuário inválido. Não foi possível enviar email.");
            return;
        }

        var viewModel = new ClaimReceivedEmailViewModel(
            userName: user.Name ?? "Cliente",
            claimId: claimId,
            accountUrl: $"{_generalSettings.BaseUrl}/Profile/User/index.cshtml",
            supportUrl: $"{_generalSettings.BaseUrl}/Support/Contact/index.cshtml"
        );

        await EnqueueEmailFromTemplateAsync(
            recipientEmail: user.Email,
            subject: $"Notificação de Reclamação (ID: {claimId})",
            viewPath: "Pages/EmailTemplates/ClaimReceived/index.cshtml",
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
