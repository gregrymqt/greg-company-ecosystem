using Hangfire;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Job;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60])]
public class ProcessPaymentNotificationJob(
    ILogger<ProcessPaymentNotificationJob> logger,
    INotificationPayment notificationPayment,
    ICacheService cacheService
) : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData? resource)
    {
        if (resource == null || string.IsNullOrEmpty(resource.Id))
        {
            logger.LogError(
                "Job de notificação de pagamento recebido com um PaymentId nulo ou vazio. O job será descartado."
            );
            return;
        }

        logger.LogInformation(
            "Iniciando processamento do job para o Payment ID: {PaymentId}",
            resource.Id
        );

        try
        {
            await notificationPayment.VerifyAndProcessNotificationAsync(resource.Id);

            logger.LogInformation(
                "Processamento do Payment ID: {PaymentId} concluído com sucesso.",
                resource.Id
            );

            var cacheKey = $"payment:db:{resource.Id}";
            await cacheService.RemoveAsync(cacheKey);
            logger.LogInformation("Cache invalidado para a chave: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar notificação para o Payment ID: {PaymentId}.",
                resource.Id
            );
            throw;
        }
    }
}
