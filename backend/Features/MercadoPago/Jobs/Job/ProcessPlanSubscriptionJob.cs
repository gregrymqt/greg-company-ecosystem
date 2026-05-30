using Hangfire;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Job;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60])]
public class ProcessPlanSubscriptionJob(
    ILogger<ProcessPlanSubscriptionJob> logger,
    IPlanUpdateNotificationService planUpdateNotificationService)
    : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData? resource)
    {
        if (resource == null || string.IsNullOrEmpty(resource.Id))
        {
            logger.LogWarning("Job recebido com ID de plano nulo ou vazio. O job será descartado.");
            return;
        }

        logger.LogInformation(
            "Iniciando processamento do job para o Plano ExternalId: {ExternalId}",
            resource.Id
        );

        try
        {
            await planUpdateNotificationService.VerifyAndProcessPlanUpdate(resource.Id);

            logger.LogInformation(
                "Processamento do Plano ExternalId: {ExternalId} concluído com sucesso.",
                resource.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar assinatura do plano com ExternalId {ExternalId}.",
                resource.Id
            );
            throw;
        }
    }
}
