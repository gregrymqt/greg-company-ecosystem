using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.ViewModels;
using static MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.ViewModels.ChargeBackViewModels;
using System.Collections.Generic;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Services;

public class ChargebackService : IChargebackService
{
    private readonly IChargebackRepository _chargebackRepository;
    private readonly IMercadoPagoChargebackIntegrationService _integrationService;

    public ChargebackService(
        IChargebackRepository chargebackRepository,
        IMercadoPagoChargebackIntegrationService integrationService)
    {
        _chargebackRepository = chargebackRepository;
        _integrationService = integrationService;
    }

    public async Task<ChargebacksIndexViewModel> GetChargebacksAsync(string? searchTerm, string? statusFilter, int page)
    {
        int pageSize = 10;
        var (chargebacks, totalCount) = await _chargebackRepository.GetPaginatedChargebacksAsync(searchTerm, statusFilter, page, pageSize);

        var viewModel = new ChargebacksIndexViewModel
        {
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            Chargebacks = chargebacks.Select(c => new ChargebackSummaryViewModel
            {
                Id = c.MercadoPagoChargebackId,
                Customer = c.User?.Name ?? c.UserId?.ToString(),
                Amount = c.Amount,
                Date = c.DateCreated != default ? c.DateCreated : c.CreatedAt,
                Status = string.IsNullOrEmpty(c.Status) ? 0 : 1, // Basic map, adjust as needed
                MercadoPagoUrl = $"https://www.mercadopago.com.br/chargebacks/{c.MercadoPagoChargebackId}"
            }).ToList()
        };

        return viewModel;
    }

    public async Task<ChargebackDetailViewModel> GetChargebackDetailAsync(string chargebackId)
    {
        var chargeback = await _chargebackRepository.GetByExternalIdAsync(chargebackId) ?? throw new ResourceNotFoundException("Chargeback não encontrado localmente.");

        var mpDetails = await _integrationService.GetChargebackDetailsFromApiAsync(chargebackId);
        
        if (mpDetails != null)
        {
            chargeback.Status = mpDetails.DocumentationStatus ?? chargeback.Status;
            chargeback.CoverageEligible = mpDetails.CoverageEligible;
            chargeback.DocumentationRequired = mpDetails.DocumentationRequired;
            
            if (mpDetails.DateCreated != default)
                chargeback.DateCreated = mpDetails.DateCreated;
                
            _chargebackRepository.Update(chargeback);
        }

        return new ChargebackDetailViewModel
        {
            ChargebackId = chargeback.MercadoPagoChargebackId,
            Valor = chargeback.Amount,
            Moeda = "BRL",
            StatusDocumentacao = chargeback.DocumentationRequired ? "Pendente" : "Enviado",
            CoberturaAplicada = chargeback.CoverageEligible,
            PrecisaDocumentacao = chargeback.DocumentationRequired,
            DataLimiteDisputa = chargeback.ExpirationDate,
            DataCriacao = chargeback.DateCreated
        };
    }

    public async Task UploadDocumentationAsync(string chargebackId, IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
            throw new AppServiceException("Nenhum arquivo enviado.");

        await _integrationService.UploadDocumentationAsync(chargebackId, files);
        
        // Update local status after successful upload
        var chargeback = await _chargebackRepository.GetByExternalIdAsync(chargebackId);
        if (chargeback != null)
        {
            chargeback.DocumentationRequired = false;
            // Optionally update internal status to indicate evidence was sent
            _chargebackRepository.Update(chargeback);
        }
    }
}
