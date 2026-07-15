using System.Threading.Tasks;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.ViewModels.ChargeBackViewModels;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Presentation.Controllers;

[Route("api/admin/chargebacks")]
[Authorize(Roles = "Admin")]
public class ChargebackController : MercadoPagoApiControllerBase
{
    private readonly IChargebackService _chargebackService;

    public ChargebackController(IChargebackService chargebackService)
    {
        _chargebackService = chargebackService;
    }

    [HttpGet]
    public async Task<IActionResult> GetChargebacks(
        [FromQuery] string? searchTerm,
        [FromQuery] string? statusFilter,
        [FromQuery] int page = 1
    )
    {
        var viewModel = await _chargebackService.GetChargebacksAsync(
            searchTerm,
            statusFilter,
            page
        );
        return Ok(viewModel);
    }

    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(ChargebackDetailViewModel), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetChargebackDetails(string id)
    {
        try
        {
            var viewModel = await _chargebackService.GetChargebackDetailAsync(id);
            return Ok(viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao obter detalhes do chargeback.");
        }
    }

    [HttpPost("{id}/documentation")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UploadDocumentation(string id)
    {
        try
        {
            var files = Request.Form.Files;
            
            if (files == null || files.Count == 0)
                return BadRequest(new { Message = "Nenhum arquivo recebido." });
                
            await _chargebackService.UploadDocumentationAsync(id, files);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao enviar documentação de defesa.");
        }
    }
}



