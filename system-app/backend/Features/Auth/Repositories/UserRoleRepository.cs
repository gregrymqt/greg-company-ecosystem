using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Auth.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Auth.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        // Use o tipo concreto do seu contexto para acessar as tabelas do Identity
        private readonly ApiDbContext _context;

        public UserRoleRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetRolesByUserIdAsync(string userId)
        {
            var query =
                from userRole in _context.UserRoles
                join role in _context.Roles on userRole.RoleId equals role.Id
                where userRole.UserId == userId
                select role.Name;

            return await query.AsNoTracking().ToListAsync();
        }
    }
}
