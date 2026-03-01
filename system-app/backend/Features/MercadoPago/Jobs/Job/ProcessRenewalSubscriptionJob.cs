using Hangfire;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Job;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60])]
public class ProcessRenewalSubscriptionJob(
    ILogger<ProcessRenewalSubscriptionJob> logger,
    ISubscriptionNotificationService subscriptionNotificationService
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData? resource)
    {
        if (resource == null || string.IsNullOrEmpty(resource.Id))
        {
            logger.LogError("Job recebido com ResourceId nulo ou vazio. O job será descartado.");
            return;
        }

        logger.LogInformation(
            "Iniciando o processamento da renovação de assinatura com PaymentId: {PaymentId}",
            resource.Id
        );

        try
        {
            await subscriptionNotificationService.ProcessRenewalAsync(resource.Id);

            logger.LogInformation(
                "Renovação da assinatura com PaymentId: {PaymentId} concluída com sucesso.",
                resource.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar renovação da assinatura com PaymentId: {PaymentId}",
                resource.Id
            );
            throw;
        }
    }
}
