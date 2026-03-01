using System.Text;
using System.Text.Json;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Plans.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Plans.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Services;

public class MercadoPagoPlanService : MercadoPagoServiceBase, IMercadoPagoPlanService
{
    public MercadoPagoPlanService(
        IHttpClientFactory httpClient,
        ILogger<IMercadoPagoPlanService> logger
    )
        : base(httpClient, logger) { }

    public async Task<PlanResponseDto> CreatePlanAsync(object payload)
    {
        const string endpoint = "/preapproval_plan";
        var responseBody = await SendMercadoPagoRequestAsync(HttpMethod.Post, endpoint, payload);
        return JsonSerializer.Deserialize<PlanResponseDto>(responseBody)
            ?? throw new InvalidOperationException();
    }

    public async Task<PlanResponseDto> UpdatePlanAsync(string externalPlanId, object payload)
    {
        var endpoint = $"/preapproval_plan/{externalPlanId}";
        var responseBody = await SendMercadoPagoRequestAsync(HttpMethod.Put, endpoint, payload);
        return JsonSerializer.Deserialize<PlanResponseDto>(responseBody)
            ?? throw new InvalidOperationException();
    }

    public async Task CancelPlanAsync(string externalPlanId)
    {
        var endpoint = $"/preapproval_plan/{externalPlanId}";
        var payload = new { status = "cancelled" };
        await SendMercadoPagoRequestAsync(HttpMethod.Put, endpoint, payload);
    }

    public async Task<PlanResponseDto> GetPlanByExternalIdAsync(string externalPlanId)
    {
        var endpoint = $"/preapproval_plan/{externalPlanId}";
        var responseBody = await SendMercadoPagoRequestAsync<object>(
            HttpMethod.Get,
            endpoint,
            null
        );
        return JsonSerializer.Deserialize<PlanResponseDto>(responseBody)
            ?? throw new InvalidOperationException();
    }

    public async Task<IEnumerable<PlanResponseDto>> SearchActivePlansAsync(
        int limit,
        int offset,
        string status,
        string sortBy,
        string criteria
    )
    {
        var queryString = new StringBuilder();
        queryString.Append($"/preapproval_plan/search?status={status}");
        queryString.Append($"&limit={limit}");
        queryString.Append($"&offset={offset}");
        queryString.Append($"&sort={sortBy}");
        queryString.Append($"&criteria={criteria}");

        var endpoint = queryString.ToString();

        var responseBody = await SendMercadoPagoRequestAsync<object>(
            HttpMethod.Get,
            endpoint,
            null
        );
        var apiResponse = JsonSerializer.Deserialize<PlanSearchResponseDto>(responseBody);

        return apiResponse?.Results?.Where(plan => plan.AutoRecurring != null) ?? [];
    }
}
