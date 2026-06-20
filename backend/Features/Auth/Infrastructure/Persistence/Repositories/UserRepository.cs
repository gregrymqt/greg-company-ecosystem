namespace MeuCrudCsharp.Features.Auth.Infrastructure.Persistence.Repositories;

using Data;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<Users> _users;

    public UserRepository(IMongoDbContext dbContext)
    {
        _users = dbContext.GetCollection<Users>("users");
    }

    public async Task<Users?> FindByGoogleIdAsync(string googleId) =>
        await _users.Find(u => u.GoogleId == googleId).FirstOrDefaultAsync();

    public async Task<Users?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public void Update(Users user)
    {
        _users.ReplaceOne(u => u.Id == user.Id, user);
    }
}
