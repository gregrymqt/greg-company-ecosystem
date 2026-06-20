using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Application.Interfaces
{
    public interface IUserAccountService
    {
        Task<AvatarUpdateResponse> UpdateProfilePictureAsync(IFormFile file);
    }
}

