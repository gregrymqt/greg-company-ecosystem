using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Presentation.Attributes;
using MeuCrudCsharp.Features.Home.Application.DTOs;
using MeuCrudCsharp.Features.Home.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Home.Presentation.Controllers;

[Route("api/[controller]")]
public class HomeController : ApiControllerBase
{
    private readonly IHomeService _service;

    public HomeController(IHomeService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetHomeContent()
    {
        try
        {
            var content = await _service.GetHomeContentAsync();
            return Ok(content);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao carregar a home.");
        }
    }

    [HttpPost("hero")]
    [Authorize(Roles = "Admin")]
    [AllowLargeFile(2048)]
    public async Task<IActionResult> CreateHero([FromForm] CreateUpdateHeroDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateHeroAsync(dto);

            if (result == null)
                return Ok(new { message = $"Chunk {dto.ChunkIndex} do Hero recebido." });

            return CreatedAtAction(nameof(GetHomeContent), null, result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao criar Hero.");
        }
    }

    [HttpPut("hero/{id}")]
    [Authorize(Roles = "Admin")]
    [AllowLargeFile(2048)]
    public async Task<IActionResult> UpdateHero(string id, [FromForm] CreateUpdateHeroDto dto)
    {
        try
        {
            bool finished = await _service.UpdateHeroAsync(id, dto);

            if (!finished)
                return Ok(new { message = $"Chunk {dto.ChunkIndex} do Hero atualizado." });

            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao atualizar Hero.");
        }
    }

    [HttpDelete("hero/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHero(string id)
    {
        try
        {
            await _service.DeleteHeroAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao deletar Hero.");
        }
    }

    [HttpPost("services")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateService([FromBody] CreateUpdateServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateServiceAsync(dto);
            return CreatedAtAction(nameof(GetHomeContent), null, result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao criar Service.");
        }
    }

    [HttpPut("services/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateService(string id, [FromBody] CreateUpdateServiceDto dto)
    {
        try
        {
            await _service.UpdateServiceAsync(id, dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao atualizar Service.");
        }
    }

    [HttpDelete("services/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(string id)
    {
        try
        {
            await _service.DeleteServiceAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao deletar Service.");
        }
    }
}

