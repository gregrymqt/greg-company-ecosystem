namespace MeuCrudCsharp.Features.Auth.Application.Interfaces;

public interface IUserContext
{
    Task<string> GetCurrentUserId();

    Task<string> GetCurrentEmail();
}
