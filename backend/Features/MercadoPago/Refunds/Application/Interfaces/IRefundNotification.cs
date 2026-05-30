namespace MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Interfaces
{
    public interface IRefundNotification
    {
        Task SendRefundStatusUpdate(string userId, string status, string message);

    }
}
