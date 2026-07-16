using MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Infrastructure.Persistence.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly ApplicationDbContext _context;

    public UserAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Users?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }
}
