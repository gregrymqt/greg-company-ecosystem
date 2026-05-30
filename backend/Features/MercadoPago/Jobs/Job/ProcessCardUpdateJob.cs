using Hangfire;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Notification.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Job;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [60])]
public class ProcessCardUpdateJob(
    ILogger<ProcessCardUpdateJob> logger,
    ICardUpdateNotificationService cardUpdateNotificationService)
    : IJob<CardUpdateNotificationPayload>
{
    public async Task ExecuteAsync(CardUpdateNotificationPayload? cardUpdateData)
    {
        if (cardUpdateData == null || string.IsNullOrEmpty(cardUpdateData.CustomerId))
        {
            logger.LogError(
                "Job de atualização de cartão recebido com payload nulo ou CustomerId inválido. O job será descartado."
            );
            return;
        }

        logger.LogInformation(
            "Iniciando processamento do job para atualização de cartão do cliente: {CustomerId}",
            cardUpdateData.CustomerId
        );

        try
        {
            await cardUpdateNotificationService.VerifyAndProcessCardUpdate(cardUpdateData);

            logger.LogInformation(
                "Processamento de atualização de cartão para o cliente {CustomerId} concluído com sucesso.",
                cardUpdateData.CustomerId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar a atualização de cartão para o cliente {CustomerId}.",
                cardUpdateData.CustomerId
            );
            throw;
        }
    }
}
