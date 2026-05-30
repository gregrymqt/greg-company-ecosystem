using System.Security.Claims;
using MeuCrudCsharp.Features.Auth.Application.DTOs;
using MeuCrudCsharp.Models; using MeuCrudCsharp.Features.Auth.Domain.Entities; // Substitua pelo seu namespace

namespace MeuCrudCsharp.Features.Auth.Application.Interfaces;

public interface IAppAuthService
{
    Task<Users> SignInWithGoogleAsync(ClaimsPrincipal googleUserPrincipal, HttpContext httpContext);

    Task<UserSessionDto> GetAuthenticatedUserDataAsync(string userId);

    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
}
