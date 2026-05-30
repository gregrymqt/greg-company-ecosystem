using MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;
using MercadoPago.Resource.User;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Features.Auth.Domain.Entities;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;

public interface IUserAccountRepository
{
    Task<Users?> GetUserByIdAsync(string userId);
}
