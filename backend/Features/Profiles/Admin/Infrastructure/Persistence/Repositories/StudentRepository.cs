using MeuCrudCsharp.Features.Profiles.Admin.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Profiles.Admin.Application.Interfaces;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.Profiles.Admin.Infrastructure.Persistence.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IMongoCollection<Users> _users;
        private readonly IMongoCollection<Subscription> _subscriptions;
        private readonly IMongoCollection<Plan> _plans;

        public StudentRepository(IMongoDbContext context)
        {
            _users = context.GetCollection<Users>("users");
            _subscriptions = context.GetCollection<Subscription>("subscriptions");
            _plans = context.GetCollection<Plan>("plans");
        }

        public async Task<(IEnumerable<Users> Items, int TotalCount)> GetAllWithSubscriptionsAsync(
            int page,
            int pageSize
        )
        {
            var filter = FilterDefinition<Users>.Empty;
            
            var totalCount = (int)await _users.CountDocumentsAsync(filter);

            if (totalCount == 0)
            {
                return (new List<Users>(), 0);
            }

            var items = await _users.Find(filter)
                .SortBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // Resolve Subscriptions and Plans manually
            var userIds = items.Select(u => u.Id).ToList();
            var subscriptions = await _subscriptions.Find(s => userIds.Contains(s.UserId)).ToListAsync();
            var planIds = subscriptions.Select(s => s.PlanId).Distinct().ToList();
            var plans = await _plans.Find(p => planIds.Contains(p.Id)).ToListAsync();

            foreach (var u in items)
            {
                u.Subscription = subscriptions.FirstOrDefault(s => s.UserId == u.Id);
                if (u.Subscription != null)
                {
                    u.Subscription.Plan = plans.FirstOrDefault(p => p.Id == u.Subscription.PlanId);
                }
            }

            return (items, totalCount);
        }

        public async Task<Users?> GetByPublicIdWithSubscriptionAsync(Guid publicId)
        {
            var user = await _users.Find(u => u.PublicId == publicId).SingleOrDefaultAsync();
            
            if (user != null)
            {
                user.Subscription = await _subscriptions.Find(s => s.UserId == user.Id).FirstOrDefaultAsync();
                if (user.Subscription != null)
                {
                    user.Subscription.Plan = await _plans.Find(p => p.Id == user.Subscription.PlanId).FirstOrDefaultAsync();
                }
            }

            return user;
        }
    }
}
