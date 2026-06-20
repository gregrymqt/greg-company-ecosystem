using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;
using MeuCrudCsharp.Models;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.ViewModels.MercadoPagoClaimsViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Application.Services;

public class UserClaimService(
    IClaimRepository claimRepository,
    IMercadoPagoIntegrationService mpService,
    IUserContext userContext)
    : IUserClaimService
{
    public async Task<List<ClaimSummaryViewModel>> GetMyClaimsAsync()
    {
        var userId = userContext.GetCurrentUserId().ToString() 
            ?? throw new UnauthorizedAccessException("UsuÃ¡rio nÃ£o autenticado.");

        var myClaims = await claimRepository.GetClaimsByUserIdAsync(userId);

        return myClaims
            .Select(c => new ClaimSummaryViewModel
            {
                InternalId = c.Id,
                MpClaimId = c.MpClaimId,
                Status = c.Status.ToString(),
                Type = c.Type.ToString(),
                DateCreated = c.DataCreated,
                IsUrgent = c.Status == InternalClaimStatus.RespondidoPeloVendedor,
            })
            .ToList();
    }

    public async Task<ClaimDetailViewModel> GetMyClaimDetailAsync(string internalId)
    {
        var userId = userContext.GetCurrentUserId().ToString();
        var claim = await claimRepository.GetByIdAsync(internalId);

        if (claim == null || claim.UserId != userId)
            throw new UnauthorizedAccessException("Essa reclamaÃ§Ã£o nÃ£o Ã© sua.");

        var messages = await mpService.GetClaimMessagesAsync(claim.MpClaimId);

        return new ClaimDetailViewModel
        {
            InternalId = claim.Id,
            MpClaimId = claim.MpClaimId,
            Status = claim.Status.ToString(),
            Messages = messages
                .Select(m => new ClaimMessageViewModel
                {
                    MessageId = m.Id,
                    SenderRole = m.SenderRole,
                    Content = m.Message,
                    DateCreated = m.DateCreated,
                    Attachments = m.Attachments?.Select(a => a.Filename).ToList() ?? [],
                    IsMe = m.SenderRole == "complainant",
                })
                .ToList(),
        };
    }

    public async Task ReplyAsync(string internalId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Mensagem nÃ£o pode ser vazia.", nameof(message));

        var userId = userContext.GetCurrentUserId().ToString();
        var claim = await claimRepository.GetByIdAsync(internalId);

        if (claim == null || claim.UserId != userId)
            throw new UnauthorizedAccessException("AÃ§Ã£o nÃ£o permitida.");

        await mpService.SendMessageAsync(claim.MpClaimId, message);
    }

    public async Task RequestMediationAsync(string internalId)
    {
        var userId = userContext.GetCurrentUserId().ToString();
        var claim = await claimRepository.GetByIdAsync(internalId);

        if (claim == null || claim.UserId != userId)
            throw new UnauthorizedAccessException("AÃ§Ã£o nÃ£o permitida.");

        await mpService.EscalateToMediationAsync(claim.MpClaimId);
    }
}

