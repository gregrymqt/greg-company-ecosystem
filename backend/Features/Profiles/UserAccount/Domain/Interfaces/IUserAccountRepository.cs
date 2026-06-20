using MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;
using MercadoPago.Resource.User;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;

public interface IUserAccountRepository
{
    Task<Users?> GetUserByIdAsync(string userId);
}

