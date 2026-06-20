using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Models.Payments> _payments;
    private readonly IMongoCollection<Users> _users;
    private readonly IMongoCollection<Subscription> _subscriptions;

    public PaymentRepository(IMongoDbContext context)
    {
        _payments = context.GetCollection<Models.Payments>("payments");
        _users = context.GetCollection<Users>("users");
        _subscriptions = context.GetCollection<Subscription>("subscriptions");
    }

    public async Task<bool> HasAnyPaymentByUserIdAsync(string userId)
    {
        return await _payments.Find(p => p.UserId == userId).AnyAsync();
    }

    public async Task<List<Models.Payments>> GetPaymentsByUserIdAndTypeAsync(
        string userId,
        string? method = null
    )
    {
        var builder = Builders<Models.Payments>.Filter;
        var filter = builder.Eq(p => p.UserId, userId);

        if (!string.IsNullOrEmpty(method))
        {
            filter &= builder.Eq(p => p.Method, method);
        }

        return await _payments.Find(filter)
            .SortByDescending(p => p.DateApproved)
            .ToListAsync();
    }

    public async Task<Models.Payments?> GetByIdWithUserAsync(string paymentId)
    {
        var payment = await _payments.Find(p => p.Id == paymentId).FirstOrDefaultAsync();
        if (payment != null && !string.IsNullOrEmpty(payment.UserId))
        {
            payment.User = await _users.Find(u => u.Id == payment.UserId).FirstOrDefaultAsync();
        }
        return payment;
    }

    public async Task<Models.Payments?> GetByExternalIdWithUserAsync(string externalPaymentId)
    {
        var payment = await _payments.Find(p => p.ExternalId == externalPaymentId).FirstOrDefaultAsync();
        if (payment != null && !string.IsNullOrEmpty(payment.UserId))
        {
            payment.User = await _users.Find(u => u.Id == payment.UserId).FirstOrDefaultAsync();
        }
        return payment;
    }

    public async Task<Models.Payments?> GetByExternalIdWithSubscriptionAsync(string externalId)
    {
        var payment = await _payments.Find(p => p.ExternalId == externalId).FirstOrDefaultAsync();
        if (payment != null && !string.IsNullOrEmpty(payment.SubscriptionId))
        {
            payment.Subscription = await _subscriptions.Find(s => s.Id == payment.SubscriptionId).FirstOrDefaultAsync();
        }
        return payment;
    }

    public void Update(Models.Payments payment)
    {
        _payments.ReplaceOne(p => p.Id == payment.Id, payment);
    }

    public async Task AddAsync(Models.Payments payment)
    {
        await _payments.InsertOneAsync(payment);
    }

    public Task Remove(Models.Payments payment)
    {
        _payments.DeleteOne(p => p.Id == payment.Id);
        return Task.CompletedTask;
    }
}
