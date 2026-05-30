namespace MeuCrudCsharp.Features.Auth.Application.Interfaces;

using System.Threading.Tasks;
using MeuCrudCsharp.Models; using MeuCrudCsharp.Features.Auth.Domain.Entities; // Substitua pelo seu namespace de Models

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(Users user);

    Task<(string Token, DateTime Expiration)> GenerateJwtTokenWithExpirationAsync(Users user);
}
