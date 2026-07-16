using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.Interfaces;
using MeuCrudCsharp.Features.Emails.Application.ViewModels;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Plans.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services;

public class PlanUpdateNotificationService(
    IPlanRepository planRepository,
    IUnitOfWork unitOfWork,
    IMercadoPagoPlanService mercadoPagoApiService,
    IRazorViewToStringRenderer razorViewToStringRenderer,
    ApplicationDbContext dbContext,
    IOptions<GeneralSettings> generalSettings,
    ILogger<PlanUpdateNotificationService> logger,
    IUserContext userContext)
    : IPlanUpdateNotificationService
{
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public async Task VerifyAndProcessPlanUpdate(string externalId)
    {
        logger.LogInformation("Iniciando verificacao e processamento para o plano com ExternalId: {ExternalId}", externalId);

        try
        {
            var mpPlan = await mercadoPagoApiService.GetPlanByExternalIdAsync(externalId);
            if (mpPlan?.Id is null || mpPlan.AutoRecurring is null)
            {
                logger.LogWarning("Nao foi possivel obter os detalhes do plano da API do Mercado Pago para o ExternalId: {ExternalId}", externalId);
                return;
            }

            var localPlan = await planRepository.GetActiveByExternalIdAsync(externalId);
            if (localPlan is null)
            {
                logger.LogWarning("Plano com ExternalId {ExternalId} encontrado na API do MP, mas nao no banco de dados local.", externalId);
                return;
            }

            var needsUpdate = false;
            var changes = new List<string>();

            var statusMapping = new Dictionary<string, bool> { { "active", true }, { "cancelled", false } };

            if (localPlan.IsActive != statusMapping[mpPlan.Status!])
            {
                localPlan.IsActive = statusMapping[mpPlan.Status!];
                changes.Add($"Status alterado para '{mpPlan.Status}'.");
                needsUpdate = true;
            }

            if (localPlan.TransactionAmount != mpPlan.AutoRecurring.TransactionAmount)
            {
                localPlan.TransactionAmount = mpPlan.AutoRecurring.TransactionAmount;
                changes.Add($"Valor alterado para '{mpPlan.AutoRecurring.TransactionAmount:C}'.");
                needsUpdate = true;
            }

            if (localPlan.FrequencyInterval != mpPlan.AutoRecurring.Frequency)
            {
                localPlan.FrequencyInterval = mpPlan.AutoRecurring.Frequency;
                changes.Add($"Frequencia alterada para '{mpPlan.AutoRecurring.Frequency}'.");
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                planRepository.Update(localPlan);

                await EnqueueAdminNotificationEmailAsync(
                    adminEmail: await userContext.GetCurrentEmail(),
                    planName: localPlan.Name,
                    externalId: externalId,
                    changes: changes
                );

                await unitOfWork.CommitAsync();
                logger.LogInformation("Plano {ExternalId} foi atualizado no banco de dados.", externalId);
            }
            else
            {
                logger.LogInformation("Nenhuma atualizacao necessaria para o plano {ExternalId}.", externalId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar atualizacao do plano {ExternalId}", externalId);
            throw;
        }
    }

    private async Task EnqueueAdminNotificationEmailAsync(string adminEmail, string planName, string externalId, List<string> changes)
    {
        var viewModel = new PlanUpdateAdminNotificationViewModel(planName, externalId, changes, $"{_generalSettings.BaseUrl}/Admin/Dashboard");
        await EnqueueEmailFromTemplateAsync(adminEmail, $"[Alerta] O Plano '{planName}' foi atualizado automaticamente", "Pages/EmailTemplates/PlanUpdate/AdminNotification.cshtml", viewModel);
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
