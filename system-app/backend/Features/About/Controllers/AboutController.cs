using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Files.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.About.Controllers;

[Route("api/[controller]")]
public class AboutController : ApiControllerBase
{
    private readonly IAboutService _service;

    public AboutController(IAboutService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAboutPageContent()
    {
        try
        {
            var content = await _service.GetAboutPageContentAsync();
            return Ok(content);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao carregar a página Sobre.");
        }
    }

    [HttpPost("sections")]
    [AllowLargeFile(2048)]
    public async Task<IActionResult> CreateSection([FromForm] CreateUpdateAboutSectionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateSectionAsync(dto);

            if (result == null)
            {
                return Ok(new { message = $"Chunk {dto.ChunkIndex} recebido." });
            }

            return CreatedAtAction(nameof(GetAboutPageContent), null, result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao criar a seção.");
        }
    }

    [HttpPut("sections/{id}")]
    [AllowLargeFile(2048)]
    public async Task<IActionResult> UpdateSection(
        int id,
        [FromForm] CreateUpdateAboutSectionDto dto
    )
    {
        try
        {
            bool finished = await _service.UpdateSectionAsync(id, dto);

            if (!finished)
            {
                return Ok(new { message = $"Chunk {dto.ChunkIndex} atualizado." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao atualizar a seção.");
        }
    }

    [HttpDelete("sections/{id}")]
    public async Task<IActionResult> DeleteSection(int id)
    {
        try
        {
            await _service.DeleteSectionAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao deletar a seção.");
        }
    }

    [HttpPost("team")]
    [AllowLargeFile(2048)]
    public async Task<IActionResult> CreateTeamMember([FromForm] CreateUpdateTeamMemberDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateTeamMemberAsync(dto);

            if (result == null)
            {
                return Ok(new { message = $"Chunk {dto.ChunkIndex} recebido." });
            }

            return CreatedAtAction(nameof(GetAboutPageContent), null, result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao criar o membro da equipe.");
        }
    }

    [HttpPut("team/{id}")]
    [AllowLargeFile(2048)]
    public async Task<IActionResult> UpdateTeamMember(
        int id,
        [FromForm] CreateUpdateTeamMemberDto dto
    )
    {
        try
        {
            bool finished = await _service.UpdateTeamMemberAsync(id, dto);
            if (!finished)
            {
                return Ok(new { message = $"Chunk {dto.ChunkIndex} atualizado." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao atualizar o membro da equipe.");
        }
    }

    [HttpDelete("team/{id}")]
    public async Task<IActionResult> DeleteTeamMember(int id)
    {
        try
        {
            await _service.DeleteTeamMemberAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao deletar o membro da equipe.");
        }
    }
}
