using MeuCrudCsharp.Features.Profiles.Admin.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Profiles.Admin.Infrastructure.Persistence.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Users> Items, int TotalCount)> GetAllWithSubscriptionsAsync(
            int page,
            int pageSize
        )
        {
            var query = _context.Users.AsQueryable();

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return (new List<Users>(), 0);
            }

            var items = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = items.Select(u => u.Id).ToList();
            var subscriptions = await _context.Subscriptions
                .Where(s => userIds.Contains(s.UserId))
                .ToListAsync();
            var planIds = subscriptions.Select(s => s.PlanId).Distinct().ToList();
            var plans = await _context.Plans
                .Where(p => planIds.Contains(p.Id))
                .ToListAsync();

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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PublicId == publicId);

            if (user != null)
            {
                user.Subscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (user.Subscription != null)
                {
                    user.Subscription.Plan = await _context.Plans
                        .FirstOrDefaultAsync(p => p.Id == user.Subscription.PlanId);
                }
            }

            return user;
        }
    }
}
