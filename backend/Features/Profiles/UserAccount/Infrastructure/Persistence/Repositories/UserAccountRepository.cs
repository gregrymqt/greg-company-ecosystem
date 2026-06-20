using MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.Interfaces;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Infrastructure.Persistence.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly IMongoCollection<Users> _users;

    public UserAccountRepository(IMongoDbContext context)
    {
        _users = context.GetCollection<Users>("users");
    }

    public async Task<Users?> GetUserByIdAsync(string userId)
    {
        return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }
}
