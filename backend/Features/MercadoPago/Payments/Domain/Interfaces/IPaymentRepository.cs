using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
namespace MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<bool> HasAnyPaymentByUserIdAsync(string userId);

    Task<List<MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments>> GetPaymentsByUserIdAndTypeAsync(
        string userId,
        string? method = null
    );

    Task<MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments?> GetByIdWithUserAsync(string paymentId);
    Task<MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments?> GetByExternalIdWithUserAsync(string externalPaymentId);

    Task<MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments?> GetByExternalIdWithSubscriptionAsync(string externalId);

    void Update(MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments payment);

    Task AddAsync(MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments payment);

    Task Remove(MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments payment);

    Task<(List<MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments> Items, long TotalCount)> GetAdminPaymentsPaginatedAsync(int page, int pageSize, string? status, string? search);
}
