using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.Services;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.DTOs.MercadoPagoClaimsDTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Presentation.Controllers;

[Route("api/admin/claims")]
[Authorize(Roles = "Admin")]
public class AdminClaimsController : MercadoPagoApiControllerBase
{
    private readonly IAdminClaimService _adminClaimService;

    public AdminClaimsController(IAdminClaimService adminClaimService)
    {
        _adminClaimService = adminClaimService;
    }

    [HttpGet]
    public async Task<ActionResult<MercadoPagoClaimsViewModels.ClaimsIndexViewModel>> GetClaims(
        [FromQuery] string? searchTerm,
        [FromQuery] string? statusFilter,
        [FromQuery] int page = 1
    )
    {
        var result = await _adminClaimService.GetClaimsAsync(searchTerm, statusFilter, page);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClaimDetails(string id)
    {
        try
        {
            var result = await _adminClaimService.GetClaimDetailsAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao obter detalhes da reclamação.");
        }
    }

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> ReplyToClaim(
        string id,
        [FromBody] MercadoPagoClaimsViewModels.ReplyClaimViewModel model
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _adminClaimService.ReplyToClaimAsync(id, model.Message);
            return Ok(new { message = "Resposta enviada com sucesso." });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao enviar resposta.");
        }
    }
}




