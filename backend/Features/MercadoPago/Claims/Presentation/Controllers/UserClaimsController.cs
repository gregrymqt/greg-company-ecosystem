using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Claims.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static MeuCrudCsharp.Features.MercadoPago.Claims.Application.DTOs.MercadoPagoClaimsDTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Presentation.Controllers;

[Route("api/user/claims")]
public class UserClaimsController : MercadoPagoApiControllerBase
{
    private readonly IUserClaimService _userClaimService;

    public UserClaimsController(IUserClaimService userClaimService)
    {
        _userClaimService = userClaimService;
    }

    [HttpGet]
    public async Task<
        ActionResult<List<MercadoPagoClaimsViewModels.ClaimSummaryViewModel>>
    > GetMyClaims()
    {
        var result = await _userClaimService.GetMyClaimsAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMyClaimDetail(string id)
    {
        try
        {
            var result = await _userClaimService.GetMyClaimDetailAsync(id);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao obter detalhe da reclamação.");
        }
    }

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(string id, [FromBody] ReplyRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("A mensagem não pode ser vazia.");

        try
        {
            await _userClaimService.ReplyAsync(id, request.Message);
            return Ok(new { message = "Mensagem enviada." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao enviar resposta.");
        }
    }

    [HttpPost("{id}/mediation")]
    public async Task<IActionResult> RequestMediation(string id)
    {
        try
        {
            await _userClaimService.RequestMediationAsync(id);
            return Ok(new { message = "Mediação solicitada ao Mercado Pago." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}




