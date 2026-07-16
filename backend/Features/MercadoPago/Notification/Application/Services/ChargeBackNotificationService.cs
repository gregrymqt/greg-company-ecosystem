using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class ChargeBackNotificationService(
    IChargebackRepository chargebackRepository,
    IPaymentRepository paymentRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    ILogger<ChargeBackNotificationService> logger,
    ApplicationDbContext dbContext,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    IOptions<GeneralSettings> generalSettings,
    IMercadoPagoChargebackIntegrationService mpIntegrationService)
    : IChargeBackNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task VerifyAndProcessChargeBackAsync(ChargebackNotificationPayload chargebackData)
    {
        var mpDetails = await mpIntegrationService.GetChargebackDetailsFromApiAsync(chargebackData.Id)
            ?? throw new Exception($"Chargeback {chargebackData.Id} nao encontrado na API do Mercado Pago.");

        var paymentIdStr = mpDetails.Payments?.FirstOrDefault()?.Id;
        if (string.IsNullOrEmpty(paymentIdStr))
        {
            logger.LogError("Chargeback {Id} sem pagamentos vinculados.", chargebackData.Id);
            return;
        }

        var mpPaymentId = long.Parse(paymentIdStr);
        var mpChargebackId = long.Parse(mpDetails.Id);

        try
        {
            logger.LogInformation("Processando Chargeback {CId} para Pagamento {PId}", mpChargebackId, mpPaymentId);

            var payment = await paymentRepository.GetByExternalIdWithUserAsync(paymentIdStr);

            if (payment != null)
            {
                payment.Status = "chargeback";
                paymentRepository.Update(payment);

                if (payment.SubscriptionId != Guid.Empty)
                {
                    var subscription = await subscriptionRepository.GetByIdAsync(payment.SubscriptionId.ToString());
                    if (subscription != null)
                    {
                        subscription.Status = "cancelled";
                        subscriptionRepository.Update(subscription);

                        var outboxEvent = new OutboxEvent
                        {
                            EventType = "subscription.status.changed",
                            Payload = JsonSerializer.Serialize(new { userId = payment.UserId, status = "canceled", planName = subscription.Plan?.Name ?? "" })
                        };
                        await dbContext.OutboxEvents.AddAsync(outboxEvent);
                    }
                }
            }

            var existingChargeback = await chargebackRepository.GetByExternalIdAsync(mpChargebackId.ToString());

            if (existingChargeback == null)
            {
                var newChargeback = new Chargeback
                {
                    MercadoPagoChargebackId = mpChargebackId.ToString(),
                    PaymentId = mpPaymentId.ToString(),
                    UserId = payment?.UserId,
                    Amount = mpDetails.Amount,
                    Status = ChargebackStatus.Novo.ToString(),
                    CreatedAt = DateTime.UtcNow,
                };
                await chargebackRepository.AddAsync(newChargeback);
            }
            else
            {
                existingChargeback.Amount = mpDetails.Amount;
                chargebackRepository.Update(existingChargeback);
            }

            if (payment?.User != null && !string.IsNullOrEmpty(payment.User.Email))
            {
                await EnqueueChargebackReceivedEmailAsync(payment.User, mpChargebackId);
            }

            await unitOfWork.CommitAsync();
            logger.LogInformation("Chargeback {Id} processado com sucesso.", mpChargebackId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar Chargeback {Id}.", mpChargebackId);
            throw;
        }
    }

    private async Task EnqueueChargebackReceivedEmailAsync(Users user, long chargebackId)
    {
        if (string.IsNullOrEmpty(user.Email)) return;

        var viewModel = new ChargebackReceivedEmailViewModel(user.Name ?? "Cliente", chargebackId, $"{_generalSettings.BaseUrl}/Support/Contact/index.cshtml");
        var htmlBody = await razorViewToStringRenderer.RenderViewToStringAsync("Pages/EmailTemplates/ChargebackReceived/index.cshtml", viewModel);

        var outboxEvent = new OutboxEvent
        {
            EventType = "email.send.requested",
            Payload = JsonSerializer.Serialize(new { To = user.Email, Subject = $"Notificacao de Contestacao (ID: {chargebackId})", HtmlBody = htmlBody, PlainTextBody = string.Empty })
        };

        await dbContext.OutboxEvents.AddAsync(outboxEvent);
    }
}
