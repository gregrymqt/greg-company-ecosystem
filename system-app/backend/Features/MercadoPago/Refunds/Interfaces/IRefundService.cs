using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.MercadoPago.Refunds.Interfaces
{
    public interface IRefundService
    {
        Task RequestRefundAsync(long paymentId);
    }
}
