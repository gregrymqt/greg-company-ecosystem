using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Application.Interfaces
{
    public interface IUserAccountService
    {
        Task<AvatarUpdateResponse> UpdateProfilePictureAsync(IFormFile file);
    }
}
