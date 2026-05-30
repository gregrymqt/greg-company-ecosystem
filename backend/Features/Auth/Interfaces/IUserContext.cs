namespace MeuCrudCsharp.Features.Auth.Interfaces;

public interface IUserContext
{
    Task<string> GetCurrentUserId();

    Task<string> GetCurrentEmail();
}
