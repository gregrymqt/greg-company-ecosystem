using System.Security.Claims;
using MeuCrudCsharp.Features.Auth.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MeuCrudCsharp.Features.Auth.Application.Interfaces;

public interface IAppAuthService
{
    Task<Users> SignInWithGoogleAsync(ClaimsPrincipal googleUserPrincipal, HttpContext httpContext);

    Task<UserSessionDto> GetAuthenticatedUserDataAsync(Guid userId);

    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
}

