namespace MeuCrudCsharp.Features.Auth.Application.Interfaces;

using System.Threading.Tasks;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities; using MeuCrudCsharp.Features.Auth.Domain.Entities; // Substitua pelo seu namespace de Models

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(Users user);

    Task<(string Token, DateTime Expiration)> GenerateJwtTokenWithExpirationAsync(Users user);
}

