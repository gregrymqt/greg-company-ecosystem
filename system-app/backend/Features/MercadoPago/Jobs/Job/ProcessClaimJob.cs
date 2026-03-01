using Hangfire;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Job;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60])]
public class ProcessClaimJob(
    ILogger<ProcessClaimJob> logger,
    IClaimNotificationService claimNotification)
    : IJob<ClaimNotificationPayload>
{
    public async Task ExecuteAsync(ClaimNotificationPayload? claimPayload)
    {
        if (claimPayload == null || string.IsNullOrEmpty(claimPayload.Id))
        {
            logger.LogError("Job de Claim recebido com payload nulo ou ID inválido. O job será descartado.");
            return;
        }

        logger.LogInformation("Iniciando processamento do job para a Claim ID: {ClaimId}", claimPayload.Id);

        try
        {
            if (!long.TryParse(claimPayload.Id, out _))
            {
                logger.LogError("ID da Claim não é um número válido: {Id}", claimPayload.Id);
                return;
            }

            await claimNotification.VerifyAndProcessClaimAsync(claimPayload);

            logger.LogInformation("Processamento da Claim ID: {ClaimId} concluído com sucesso.", claimPayload.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar a notificação para a Claim ID: {ClaimId}.", claimPayload.Id);
            throw;
        }
    }
}