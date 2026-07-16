using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Data;
using Microsoft.AspNetCore.Identity;

namespace MeuCrudCsharp.Features.Auth.Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly UserManager<Users> _userManager;

        public UserRoleRepository(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<string>> GetRolesByUserIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }
    }
}
