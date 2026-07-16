using System.Security.Claims;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Support.Application.DTOs;
using MeuCrudCsharp.Features.Support.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Support.Presentation.Controllers
{
    [Route("api/admin/support/tickets")]
    [Authorize(Roles = "Admin")]
    public class AdminSupportController : ApiControllerBase
    {
        private readonly ISupportService _service;

        public AdminSupportController(ISupportService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetAllTicketsPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar tickets.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var ticket = await _service.GetTicketByIdAsync(id);
                return Ok(new { success = true, data = ticket });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar ticket.");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSupportTicketDto dto)
        {
            try
            {
                await _service.UpdateTicketStatusAsync(id, dto);
                return Ok(new { success = true, message = "Status atualizado." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao atualizar ticket.");
            }
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTicket(Guid id)
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new { success = false, message = "Usuário não identificado." });

                await _service.AssignTicketAsync(id, adminId);
                return Ok(new { success = true, message = "Ticket atribuído com sucesso." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao atribuir ticket.");
            }
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> Reply(Guid id, [FromBody] ReplyToTicketDto dto)
        {
            try
            {
                var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminIdStr) || !Guid.TryParse(adminIdStr, out var adminId))
                    return Unauthorized(new { success = false, message = "Usuário não identificado." });

                await _service.ReplyToTicketAsync(id, adminId, "Admin", dto);
                return Ok(new { success = true, message = "Resposta enviada com sucesso." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao enviar resposta.");
            }
        }
    }
}
