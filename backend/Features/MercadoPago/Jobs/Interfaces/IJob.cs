namespace MeuCrudCsharp.Features.MercadoPago.Jobs.Interfaces
{
    public interface IJob<in TResource>
    {
        Task ExecuteAsync(TResource resource);
    }
}
