namespace MeuCrudCsharp.Features.Auth.Application.Interfaces;

using System.Threading.Tasks;
using MeuCrudCsharp.Models; // Substitua pelo seu namespace de Models

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(Users user);

    Task<(string Token, DateTime Expiration)> GenerateJwtTokenWithExpirationAsync(Users user);
}
