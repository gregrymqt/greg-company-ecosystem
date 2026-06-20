using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
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

        public async Task<List<string>> GetRolesByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }
    }
}
