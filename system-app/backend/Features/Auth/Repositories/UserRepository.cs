namespace MeuCrudCsharp.Features.Auth.Repositories;

using Data;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

public class UserRepository(ApiDbContext dbContext) : IUserRepository
{
    public async Task<Users?> FindByGoogleIdAsync(string googleId) =>
        await dbContext.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

    public async Task<Users?> GetByIdAsync(string id) =>
        await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

    public void Update(Users user)
    {
        dbContext.Users.Update(user);
    }
}
