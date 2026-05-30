namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Interfaces
{
    public interface IQueueService
    {
        Task EnqueueJobAsync<TJob, TResource>(TResource resource)
            where TJob : IJob<TResource>;
    }
}
