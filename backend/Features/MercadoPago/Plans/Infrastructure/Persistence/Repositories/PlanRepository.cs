using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Plans.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Infrastructure.Persistence.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly IMongoCollection<Plan> _plans;

    public PlanRepository(IMongoDbContext context)
    {
        _plans = context.GetCollection<Plan>("plans");
    }

    public async Task AddAsync(Plan plan)
    {
        await _plans.InsertOneAsync(plan);
    }

    public void Update(Plan plan)
    {
        _plans.ReplaceOne(p => p.Id == plan.Id, plan);
    }

    public void Remove(object payload)
    {
        if (payload is Plan plan)
        {
            _plans.DeleteOne(p => p.Id == plan.Id);
        }
    }

    public async Task<Plan?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true)
    {
        return await _plans.Find(p => p.PublicId == publicId).FirstOrDefaultAsync();
    }

    public async Task<Plan?> GetActiveByExternalIdAsync(string externalId)
    {
        return await _plans.Find(p => p.IsActive && p.ExternalPlanId == externalId).FirstOrDefaultAsync();
    }

    public async Task<PagedResultDto<Plan>> GetActivePlansAsync(int page, int pageSize)
    {
        var filter = Builders<Plan>.Filter.Eq(p => p.IsActive, true);
        
        var totalCount = (int)await _plans.CountDocumentsAsync(filter);

        var items = await _plans.Find(filter)
            .SortBy(p => p.TransactionAmount)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedResultDto<Plan>(items, page, pageSize, totalCount);
    }

    public async Task<List<Plan>> GetByExternalIdsAsync(IEnumerable<string> externalIds)
    {
        var filter = Builders<Plan>.Filter.In(p => p.ExternalPlanId, externalIds);
        return await _plans.Find(filter).ToListAsync();
    }
}

