using System;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.DTOs;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;

public interface IUserSubscriptionService
{
    Task<SubscriptionDetailsDto?> GetMySubscriptionDetailsAsync();
    Task ChangeSubscriptionStatusAsync(string newStatus);
}
