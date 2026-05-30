using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Plans.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Plans.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Controllers;

[Route("api/admin/plans")]
public class AdminPlansController : MercadoPagoApiControllerBase
{
    private readonly IPlanService _planService;

    public AdminPlansController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto createDto)
    {
        try
        {
            var planResponseDto = await _planService.CreatePlanAsync(createDto);

            return CreatedAtAction(
                nameof(GetPlanById),
                new { id = planResponseDto.PublicId },
                planResponseDto
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao criar plano.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        try
        {
            var plans = await _planService.GetActiveApiPlansAsync(page, pageSize);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao buscar os planos.");
        }
    }

    [HttpGet("{id}")]
    [ActionName(nameof(GetPlanById))]
    public async Task<IActionResult> GetPlanById(string id)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var planEditDto = await _planService.GetPlanEditDtoByIdAsync(guidId);
            if (planEditDto == null)
            {
                return NotFound(new { message = $"Plano com ID {id} não encontrado." });
            }
            return Ok(planEditDto);
        }
        return BadRequest(new { message = "ID inválido." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlan(string id, [FromBody] UpdatePlanDto updateDto)
    {
        try
        {
            if (Guid.TryParse(id, out var guidId))
            {
                var updatedPlan = await _planService.UpdatePlanAsync(guidId, updateDto);
                return Ok(updatedPlan);
            }
            return BadRequest(new { message = "ID inválido." });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao atualizar o plano.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlan(Guid id)
    {
        try
        {
            await _planService.DeletePlanAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao deletar o plano.");
        }
    }
}
