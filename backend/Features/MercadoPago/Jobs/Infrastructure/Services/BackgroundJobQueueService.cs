using Hangfire;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Interfaces;

namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Infrastructure.Services;

public class BackgroundJobQueueService(
    IBackgroundJobClient backgroundJobClient,
    ILogger<BackgroundJobQueueService> logger)
    : IQueueService
{
    public Task EnqueueJobAsync<TJob, TResource>(TResource resource)
        where TJob : IJob<TResource>
    {
        if (resource == null)
        {
            throw new ArgumentNullException(
                nameof(resource),
                "O recurso (payload) não pode ser nulo."
            );
        }

        try
        {
            var jobName = typeof(TJob).Name;
            logger.LogInformation(
                "Enfileirando job do tipo '{JobName}' com o payload: {Payload}",
                jobName,
                resource
            );

            backgroundJobClient.Enqueue<TJob>(job => job.ExecuteAsync(resource));

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao enfileirar o job do tipo {JobName}. O job NÃO foi agendado.",
                typeof(TJob).Name
            );
            throw new AppServiceException("Falha ao agendar a tarefa de processamento.", ex);
        }
    }
}
