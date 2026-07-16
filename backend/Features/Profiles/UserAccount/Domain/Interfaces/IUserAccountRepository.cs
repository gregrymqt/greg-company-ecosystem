using MeuCrudCsharp.Features.Auth.Domain.Entities;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;

public interface IUserAccountRepository
{
    Task<Users?> GetUserByIdAsync(Guid userId);
}
