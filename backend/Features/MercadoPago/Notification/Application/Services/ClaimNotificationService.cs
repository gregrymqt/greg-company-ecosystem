// Service for handling Mercado Pago claim notifications.
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Data;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class ClaimNotificationService(
    IClaimRepository claimRepository,
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    ILogger<ClaimNotificationService> logger,
    ApplicationDbContext dbContext,
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
                logger.LogWarning("Id da claim invalido.");
                return;
            }

            if (!long.TryParse(claimPayload.Id, out var mpClaimId))
            {
                logger.LogError("ID da Claim nao e um numero valido: {Id}", claimPayload.Id);
                return;
            }

            var claimDetails = await mpIntegrationService.GetClaimByIdAsync(mpClaimId.ToString());
            if (claimDetails == null)
            {
                logger.LogError("Nao foi possivel obter detalhes da Claim {Id} na API do MP.", mpClaimId);
                return;
            }

            var resourceId = claimDetails.ResourceId;
            var resourceTypeEnum = claimDetails.Resource;

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

            var existingClaim = await claimRepository.GetByMpClaimIdAsync(mpClaimId.ToString());

            if (existingClaim == null)
            {
                var newClaimRecord = new Claim
                {
                    MercadoPagoClaimId = mpClaimId.ToString(),
                    PaymentId = resourceId,
                    Type = claimDetails.Type,
                    ResourceType = resourceTypeEnum,
                    UserId = user?.Id,
                    DateCreated = DateTime.UtcNow,
                    Status = ClaimStatus.Opened,
                    CurrentStage = claimDetails.Stage
                };

                await claimRepository.AddAsync(newClaimRecord);
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
        if (string.IsNullOrEmpty(user.Email)) return;

        var viewModel = new ClaimReceivedEmailViewModel(user.Name ?? "Cliente", claimId, $"{_generalSettings.BaseUrl}/Profile/User/index.cshtml", $"{_generalSettings.BaseUrl}/Support/Contact/index.cshtml");
        await EnqueueEmailFromTemplateAsync(user.Email, $"Notificacao de Reclamacao (ID: {claimId})", "Pages/EmailTemplates/ClaimReceived/index.cshtml", viewModel);
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
