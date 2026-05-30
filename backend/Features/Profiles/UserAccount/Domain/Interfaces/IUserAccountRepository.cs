using MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;
using MercadoPago.Resource.User;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;

public interface IUserAccountRepository
{
    Task<Users?> GetUserByIdAsync(string userId);
}
