using System;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.DTOs.MercadoPagoClaimsDTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;

public interface IMercadoPagoIntegrationService
{
    Task<MpClaimSearchResponse> SearchClaimsAsync(string role, int offset = 0, int limit = 30);

    Task<List<MpMessageResponse>> GetClaimMessagesAsync(string claimId);

    Task<MpClaimItem?> GetClaimByIdAsync(string claimId);

    Task SendMessageAsync(
        string claimId,
        string message,
        List<string>? attachments = null,
        string receiverRole = "complainant"
    );

    Task EscalateToMediationAsync(string claimId);
}
