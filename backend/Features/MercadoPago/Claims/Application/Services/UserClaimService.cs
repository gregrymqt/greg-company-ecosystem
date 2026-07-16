using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.ViewModels.MercadoPagoClaimsViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Application.Services;

public class UserClaimService(
    IClaimRepository claimRepository,
    IMercadoPagoIntegrationService mpService,
    IUserContext userContext,
    IHubContext<GlobalRealtimeHub> hubContext)
    : IUserClaimService
{
    public async Task<List<ClaimSummaryViewModel>> GetMyClaimsAsync()
    {
        var userId = Guid.Parse(await userContext.GetCurrentUserId());

        var myClaims = await claimRepository.GetClaimsByUserIdAsync(userId);

        return myClaims
            .Select(c => new ClaimSummaryViewModel
            {
                InternalId = c.Id.ToString(),
                MpClaimId = c.MercadoPagoClaimId,
                Status = c.Status.ToString(),
                Type = c.Type.ToString(),
                DateCreated = c.DateCreated,
                IsUrgent = c.Status == ClaimStatus.Opened,
            })
            .ToList();
    }

    public async Task<ClaimDetailViewModel> GetMyClaimDetailAsync(string internalId)
    {
        var userId = Guid.Parse(await userContext.GetCurrentUserId());
        var claim = await claimRepository.GetByIdAsync(Guid.Parse(internalId));

        if (claim == null || claim.UserId != userId)
            throw new UnauthorizedAccessException("Essa reclamação não é sua.");

        var messages = await mpService.GetClaimMessagesAsync(claim.MercadoPagoClaimId);

        return new ClaimDetailViewModel
        {
            InternalId = claim.Id.ToString(),
            MpClaimId = claim.MercadoPagoClaimId,
            Status = claim.Status.ToString(),
            Messages = messages
                .Select(m => new ClaimMessageViewModel
                {
                    MessageId = m.Id,
                    SenderRole = m.SenderRole,
                    Content = m.Message,
                    DateCreated = m.DateCreated,
                    Attachments = [.. m.Attachments?.Select(a => a.Filename) ?? []],
                    IsMe = m.SenderRole == "complainant",
                })
                .ToList(),
        };
    }

    public async Task ReplyAsync(string internalId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Mensagem não pode ser vazia.", nameof(message));

        var userId = Guid.Parse(await userContext.GetCurrentUserId());
        var claim = await claimRepository.GetByIdAsync(Guid.Parse(internalId));

        if (claim == null || claim.UserId != userId)
            throw new UnauthorizedAccessException("Ação não permitida.");

        await mpService.SendMessageAsync(claim.MercadoPagoClaimId, message);

        await hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveMessage", new { claimId = claim.Id });
    }

    public async Task RequestMediationAsync(string internalId)
    {
        var userId = Guid.Parse(await userContext.GetCurrentUserId());
        var claim = await claimRepository.GetByIdAsync(Guid.Parse(internalId));

        if (claim == null || claim.UserId != userId)
            throw new UnauthorizedAccessException("Ação não permitida.");

        await mpService.EscalateToMediationAsync(claim.MercadoPagoClaimId);

        await hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveMessage", new { claimId = claim.Id });
    }
}
