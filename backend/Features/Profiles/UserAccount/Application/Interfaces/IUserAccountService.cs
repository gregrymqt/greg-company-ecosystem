using MeuCrudCsharp.Features.Profiles.UserAccount.Application.DTOs;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Application.Interfaces
{
    public interface IUserAccountService
    {
        Task<AvatarUpdateResponse> UpdateProfilePictureAsync(IFormFile file);
        Task<UserProfileDto> GetProfileAsync();
    }
}
