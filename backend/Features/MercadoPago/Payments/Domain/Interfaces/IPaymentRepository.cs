using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<bool> HasAnyPaymentByUserIdAsync(Guid userId);

    Task<List<Payment>> GetPaymentsByUserIdAndTypeAsync(
        Guid userId,
        string? method = null
    );

    Task<Payment?> GetByIdWithUserAsync(Guid paymentId);
    Task<Payment?> GetByExternalIdWithUserAsync(string externalPaymentId);

    Task<Payment?> GetByExternalIdWithSubscriptionAsync(string externalId);

    void Update(Payment payment);

    Task AddAsync(Payment payment);

    Task Remove(Payment payment);

    Task<(List<Payment> Items, long TotalCount)> GetAdminPaymentsPaginatedAsync(int page, int pageSize, string? status, string? search);
}
