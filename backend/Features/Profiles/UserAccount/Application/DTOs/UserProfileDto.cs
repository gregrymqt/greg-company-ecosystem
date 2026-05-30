namespace MeuCrudCsharp.Features.Profiles.UserAccount.Application.DTOs
{
    public class UserProfileDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string AvatarUrl { get; set; }
    }

    public class AvatarUpdateResponse
    {
        public required string AvatarUrl { get; set; }
        public required string Message { get; set; }
    }
}
