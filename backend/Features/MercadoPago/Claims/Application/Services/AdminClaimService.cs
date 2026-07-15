using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Interfaces;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.DTOs.MercadoPagoClaimsDTOs;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.ViewModels.MercadoPagoClaimsViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Application.Services;

public class AdminClaimService(
    IClaimRepository claimRepository,
    IMercadoPagoIntegrationService mpService,
    ICacheService cacheService,
    ILogger<AdminClaimService> logger,
    IHubContext<GlobalRealtimeHub> hubContext)
    : IAdminClaimService
{
    private const int PageSize = 10;
    private const string ClaimsCacheVersionKey = "claims_cache_version";

    public async Task<ClaimsIndexViewModel> GetClaimsAsync(
        string? searchTerm,
        string? statusFilter,
        int page
    )
    {
        if (page < 1)
        {
            logger.LogWarning("PÃ¡gina invÃ¡lida recebida: {Page}. Usando pÃ¡gina 1.", page);
            page = 1;
        }

        var cacheVersion = await GetCacheVersionAsync();
        var cacheKey = $"Claims_v{cacheVersion}_s:{searchTerm}_f:{statusFilter}_p:{page}";

        return await cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var (claims, totalCount) = await claimRepository.GetPaginatedClaimsAsync(
                    searchTerm,
                    statusFilter,
                    page,
                    PageSize
                );

                var claimSummaries = claims
                    .Select(c => new ClaimSummaryViewModel
                    {
                        InternalId = c.Id,
                        MpClaimId = c.MercadoPagoClaimId,
                        CustomerName = c.User?.Name ?? "Desconhecido",
                        Status = c.Status.ToString(),
                        DateCreated = c.DateCreated,
                        Type = c.Type.ToString(),
                    })
                    .ToList();

                return new ClaimsIndexViewModel
                {
                    Claims = claimSummaries,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize),
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                };
            },
            TimeSpan.FromMinutes(5)
        ) ?? new ClaimsIndexViewModel();
    }

    public async Task<ClaimDetailViewModel> GetClaimDetailsAsync(string localId)
    {
        var localClaim = await claimRepository.GetByIdAsync(localId)
            ?? throw new ResourceNotFoundException("ReclamaÃ§Ã£o nÃ£o encontrada.");

        List<MpMessageResponse> messages;
        try
        {
            logger.LogInformation("Buscando mensagens da claim {MpClaimId} na API do Mercado Pago",
                localClaim.MercadoPagoClaimId);
            messages = await mpService.GetClaimMessagesAsync(localClaim.MercadoPagoClaimId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao buscar mensagens no MP para a claim {ClaimId}",
                localClaim.MercadoPagoClaimId
            );
            messages = [];
        }

        return new ClaimDetailViewModel
        {
            InternalId = localClaim.Id,
            MpClaimId = localClaim.MercadoPagoClaimId,
            Status = localClaim.Status.ToString(),
            Messages =
            [
                .. messages.Select(m => new ClaimMessageViewModel
                {
                    MessageId = m.Id,
                    SenderRole = m.SenderRole,
                    Content = m.Message,
                    DateCreated = m.DateCreated,
                    Attachments = [.. m.Attachments?.Select(a => a.Filename) ?? []],
                })
            ],
        };
    }

    public async Task ReplyToClaimAsync(string localId, string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
            throw new ArgumentException("Mensagem nÃ£o pode ser vazia.", nameof(messageText));

        var localClaim = await claimRepository.GetByIdAsync(localId)
            ?? throw new ResourceNotFoundException("ReclamaÃ§Ã£o nÃ£o encontrada.");

        await mpService.SendMessageAsync(localClaim.MercadoPagoClaimId, messageText);

        if (!string.IsNullOrEmpty(localClaim.UserId))
        {
            await hubContext.Clients.User(localClaim.UserId).SendAsync("ReceiveMessage", new { claimId = localClaim.Id });
        }

        logger.LogInformation("Resposta enviada para a claim MP {MpId}", localClaim.MercadoPagoClaimId);
    }

    private Task<string?> GetCacheVersionAsync()
    {
        return cacheService.GetOrCreateAsync(
            ClaimsCacheVersionKey,
            () => Task.FromResult(Guid.NewGuid().ToString())
        );
    }
}
