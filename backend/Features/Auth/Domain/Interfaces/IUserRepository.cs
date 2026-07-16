using MeuCrudCsharp.Features.Auth.Domain.Entities;

namespace MeuCrudCsharp.Features.Auth.Domain.Interfaces;

public interface IUserRepository
{
    Task<Users?> FindByGoogleIdAsync(string googleId);
    Task<Users?> GetByIdAsync(Guid id);

    void Update(Users user);
}
