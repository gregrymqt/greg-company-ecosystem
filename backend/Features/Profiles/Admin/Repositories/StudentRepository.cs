using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Profiles.Admin.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Profiles.Admin.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApiDbContext _context;

        public StudentRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Users> Items, int TotalCount)> GetAllWithSubscriptionsAsync(
            int page,
            int pageSize
        )
        {
            var query = _context.Users.AsNoTracking();

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return ([], 0);
            }

            var items = await query
                .Include(u => u.Subscription)
                .ThenInclude(s => s!.Plan)
                .OrderBy(u => u.Name ?? string.Empty)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Users?> GetByPublicIdWithSubscriptionAsync(Guid publicId)
        {
            return await _context
                .Users.AsNoTracking()
                .Include(u => u.Subscription)
                .ThenInclude(s => s!.Plan)
                .SingleOrDefaultAsync(u => u.PublicId == publicId);
        }
    }
}
