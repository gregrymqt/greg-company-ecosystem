using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using System;
using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Application.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IHttpClientFactory httpClient,
    ILogger<PaymentService> logger)
    : MercadoPagoServiceBase(httpClient, logger), IPaymentService
{
    public async Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(string userId)
    {
        var payments = await paymentRepository.GetPaymentsByUserIdAndTypeAsync(Guid.Parse(userId));

        var historyDtos = payments
            .Select(p => new PaymentHistoryDto
            {
                Id = p.ExternalId ?? p.Id.ToString(),
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PaymentMethod = p.Method,

                Description = string.IsNullOrEmpty(p.Description)
                    ? $"Pagamento via {p.Method}"
                    : p.Description,
            })
            .ToList();

        return historyDtos;
    }

    public async Task<AdminPaymentPaginatedResponse> GetAdminPaymentsPaginatedAsync(int page, int pageSize, string? status, string? search)
    {
        var (items, totalCount) = await paymentRepository.GetAdminPaymentsPaginatedAsync(page, pageSize, status, search);

        var dtos = items.Select(p => new AdminPaymentItemDto
        {
            Id = p.Id.ToString(),
            Amount = p.Amount,
            NetReceivedAmount = p.NetReceivedAmount,
            Status = p.Status ?? string.Empty,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            PaymentMethod = p.Method,
            PayerEmail = p.PayerEmail,
            UserId = p.UserId.ToString()
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new AdminPaymentPaginatedResponse
        {
            Items = dtos,
            TotalItems = (int)totalCount,
            TotalPages = totalPages,
            CurrentPage = page
        };
    }
}
