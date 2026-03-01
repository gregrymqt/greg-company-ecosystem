using Hangfire;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Job;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60])]
public class ProcessCreateSubscriptionJob(
    ILogger<ProcessCreateSubscriptionJob> logger,
    ICacheService cache,
    ISubscriptionCreateNotificationService notificationSubscriptionCreate)
    : IJob<PaymentNotificationData>
{
    public async Task ExecuteAsync(PaymentNotificationData? resource)
    {
        if (resource == null || string.IsNullOrEmpty(resource.Id))
        {
            logger.LogError("Job recebido com ResourceId nulo ou vazio. O job será descartado.");
            return;
        }

        logger.LogInformation(
            "Iniciando processamento do job para a criação de assinatura: {ResourceId}",
            resource.Id
        );

        try
        {
            await notificationSubscriptionCreate.VerifyAndProcessSubscriptionAsync(resource.Id);

            logger.LogInformation(
                "Processamento da criação de assinatura ID: {ResourceId} concluído com sucesso.",
                resource.Id
            );

            var cacheKey = $"subscription_{resource.Id}";
            await cache.RemoveAsync(cacheKey);
            logger.LogInformation("Cache invalidado para a chave: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar a criação de assinatura ID: {ResourceId}", resource.Id);
            throw;
        }
    }
}
