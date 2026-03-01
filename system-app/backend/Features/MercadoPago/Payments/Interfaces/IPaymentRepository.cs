namespace MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;

public interface IPaymentRepository
{
    Task<bool> HasAnyPaymentByUserIdAsync(string userId);

    Task<List<Models.Payments>> GetPaymentsByUserIdAndTypeAsync(
        string userId,
        string? method = null
    );

    Task<Models.Payments?> GetByIdWithUserAsync(string paymentId);
    Task<Models.Payments?> GetByExternalIdWithUserAsync(string externalPaymentId);

    Task<Models.Payments?> GetByExternalIdWithSubscriptionAsync(string externalId);

    void Update(Models.Payments payment);

    Task AddAsync(Models.Payments payment);

    Task Remove(Models.Payments payment);
}
