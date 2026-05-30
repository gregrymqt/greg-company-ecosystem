using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Profiles.UserAccount.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Interfaces
{
    public interface IUserAccountService
    {
        Task<AvatarUpdateResponse> UpdateProfilePictureAsync(IFormFile file);
    }
}
