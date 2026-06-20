using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Infrastructure.Persistence.Repositories;

public class ChargebackRepository : IChargebackRepository
{
    private readonly IMongoCollection<Chargeback> _chargebacks;
    private readonly IMongoCollection<Users> _users;

    public ChargebackRepository(IMongoDbContext context)
    {
        _chargebacks = context.GetCollection<Chargeback>("chargebacks");
        _users = context.GetCollection<Users>("users");
    }

    public async Task<(List<Chargeback> Chargebacks, int TotalCount)> GetPaginatedChargebacksAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        int pageSize
    )
    {
        var builder = Builders<Chargeback>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (long.TryParse(searchTerm, out var idSearch))
            {
                filter &= builder.Eq(c => c.ChargebackId, idSearch);
            }
            else
            {
                // This would be expensive, let's just do a manual lookup for user IDs that match
                var userFilter = Builders<Users>.Filter.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
                var userIds = await _users.Find(userFilter).Project(u => u.Id).ToListAsync();
                filter &= builder.In(c => c.UserId, userIds);
            }
        }

        if (
            !string.IsNullOrEmpty(statusFilter)
            && Enum.TryParse<ChargebackStatus>(statusFilter, true, out var statusEnum)
        )
        {
            filter &= builder.Eq(c => c.Status, statusEnum);
        }

        var totalCount = (int)await _chargebacks.CountDocumentsAsync(filter);

        var chargebacks = await _chargebacks.Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        var chargebackUserIds = chargebacks.Where(c => !string.IsNullOrEmpty(c.UserId)).Select(c => c.UserId).Distinct().ToList();
        if (chargebackUserIds.Any())
        {
            var users = await _users.Find(u => chargebackUserIds.Contains(u.Id)).ToListAsync();
            foreach (var c in chargebacks)
            {
                if (!string.IsNullOrEmpty(c.UserId))
                {
                    c.User = users.FirstOrDefault(u => u.Id == c.UserId);
                }
            }
        }

        return (chargebacks, totalCount);
    }

    public async Task<bool> ExistsByExternalIdAsync(long chargebackId)
    {
        return await _chargebacks.Find(c => c.ChargebackId == chargebackId).AnyAsync();
    }

    public async Task<Chargeback?> GetByExternalIdAsync(long chargebackId)
    {
        return await _chargebacks.Find(c => c.ChargebackId == chargebackId).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Chargeback chargeback)
    {
        await _chargebacks.InsertOneAsync(chargeback);
    }

    public void Update(Chargeback chargeback)
    {
        _chargebacks.ReplaceOne(c => c.Id == chargeback.Id, chargeback);
    }
}

