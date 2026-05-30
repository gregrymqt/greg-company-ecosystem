namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Interfaces
{
    public interface IJob<in TResource>
    {
        Task ExecuteAsync(TResource resource);
    }
}
