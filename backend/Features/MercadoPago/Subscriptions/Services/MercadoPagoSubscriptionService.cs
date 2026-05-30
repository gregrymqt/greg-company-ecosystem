namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Services;

using System.Text.Json;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Models.Enums;

public class MercadoPagoSubscriptionService
    : MercadoPagoServiceBase,
        IMercadoPagoSubscriptionService
{
    public MercadoPagoSubscriptionService(
        IHttpClientFactory httpClient,
        ILogger<MercadoPagoSubscriptionService> logger
    )
        : base(httpClient, logger) { }

    public async Task<SubscriptionResponseDto> CreateSubscriptionAsync(
        CreateSubscriptionDto payload
    )
    {
        const string endpoint = "/preapproval";
        var responseBody = await SendMercadoPagoRequestAsync(HttpMethod.Post, endpoint, payload);

        return JsonSerializer.Deserialize<SubscriptionResponseDto>(responseBody)
            ?? throw new AppServiceException(
                "Falha ao desserializar a resposta de criação de assinatura."
            );
    }

    public async Task<SubscriptionResponseDto?> GetSubscriptionByIdAsync(string subscriptionId)
    {
        var endpoint = $"/preapproval/{subscriptionId}";

        var responseBody = await SendMercadoPagoRequestAsync(
            HttpMethod.Get,
            endpoint,
            (object?)null
        );

        return JsonSerializer.Deserialize<SubscriptionResponseDto>(responseBody)
            ?? throw new ResourceNotFoundException("Assinatura não encontrada no Mercado Pago.");
    }

    public async Task UpdateSubscriptionCardAsync(string subscriptionId, string newCardToken)
    {
        var endpoint = $"/preapproval/{subscriptionId}";
        var payload = new { card_token_id = newCardToken };

        await SendMercadoPagoRequestAsync(HttpMethod.Put, endpoint, payload);
    }

    public async Task<SubscriptionResponseDto> UpdateSubscriptionValueAsync(
        string subscriptionId,
        UpdateSubscriptionValueDto dto
    )
    {
        var endpoint = $"/preapproval/{subscriptionId}";

        var payload = new { auto_recurring = new { transaction_amount = dto.TransactionAmount } };

        var responseBody = await SendMercadoPagoRequestAsync(HttpMethod.Put, endpoint, payload);

        return JsonSerializer.Deserialize<SubscriptionResponseDto>(responseBody)
            ?? throw new AppServiceException("Falha ao atualizar valor no MP.");
    }

    public async Task<SubscriptionResponseDto> UpdateSubscriptionStatusAsync(
        string subscriptionId,
        UpdateSubscriptionStatusDto dto
    )
    {
        var endpoint = $"/preapproval/{subscriptionId}";
        var payload = new { status = dto.Status };

        var responseBody = await SendMercadoPagoRequestAsync(HttpMethod.Put, endpoint, payload);

        return JsonSerializer.Deserialize<SubscriptionResponseDto>(responseBody)
            ?? throw new AppServiceException("Falha ao atualizar status no MP.");
    }

    public async Task CancelSubscriptionAsync(string subscriptionId)
    {
        await UpdateSubscriptionStatusAsync(
            subscriptionId,
            new UpdateSubscriptionStatusDto(SubscriptionStatus.Cancelled.ToMpString())
        );
    }
}
