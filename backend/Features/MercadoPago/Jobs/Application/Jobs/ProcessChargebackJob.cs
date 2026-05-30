using Hangfire;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Jobs;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60])]
public class ProcessChargebackJob(
    ILogger<ProcessChargebackJob> logger,
    IChargeBackNotificationService chargeBackNotificationService)
    : IJob<ChargebackNotificationPayload>
{
    public async Task ExecuteAsync(ChargebackNotificationPayload? chargebackData)
    {
        if (chargebackData == null || string.IsNullOrEmpty(chargebackData.Id))
        {
            logger.LogError("Job de Chargeback recebido com payload inválido.");
            return;
        }

        logger.LogInformation(
            "Iniciando processamento do job para o Chargeback ID: {ChargebackId}", 
            chargebackData.Id
        );

        try
        {
            if (!long.TryParse(chargebackData.Id, out _))
            {
                logger.LogError(
                    "ID do Chargeback não é um número válido: {Id}", 
                    chargebackData.Id
                );
                return;
            }

            await chargeBackNotificationService.VerifyAndProcessChargeBackAsync(chargebackData);

            logger.LogInformation("Job Chargeback {Id} concluído com sucesso.", chargebackData.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro no Job Chargeback {Id}.", chargebackData.Id);
            throw;
        }
    }
}