using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Interfaces
{
    public interface IRefundService
    {
        Task RequestRefundAsync(long paymentId);
    }
}
