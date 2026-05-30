using MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.Interfaces;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Infrastructure.Persistence.Repositories;

public class UserAccountRepository(ApiDbContext context) : IUserAccountRepository
{
    public async Task<Users?> GetUserByIdAsync(string userId)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}
