namespace MeuCrudCsharp.Features.Auth.Infrastructure.Persistence.Repositories;

using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Users?> FindByGoogleIdAsync(string googleId) =>
        await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

    public async Task<Users?> GetByIdAsync(Guid id) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public void Update(Users user)
    {
        _context.Users.Update(user);
    }
}
