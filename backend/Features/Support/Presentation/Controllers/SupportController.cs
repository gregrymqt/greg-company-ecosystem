using System.Security.Claims;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Support.Application.DTOs;
using MeuCrudCsharp.Features.Support.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Support.Presentation.Controllers
{
    [Route("api/support")]
    public class SupportController : ApiControllerBase
    {
        private readonly ISupportService _service;

        public SupportController(ISupportService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupportTicketDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { success = false, message = "Usuário não identificado." });

                await _service.CreateTicketAsync(userId, dto);
                return Created("", new { success = true, message = "Ticket de suporte criado com sucesso." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao criar ticket.");
            }
        }

        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { success = false, message = "Usuário não identificado." });

                var tickets = await _service.GetTicketsByUserIdAsync(userId);
                return Ok(new { success = true, data = tickets });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar seus tickets.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { success = false, message = "Usuário não identificado." });

                var ticket = await _service.GetTicketByIdAsync(id);
                if (ticket.UserId != userId)
                    return Forbid();

                return Ok(new { success = true, data = ticket });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar ticket.");
            }
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> Reply(Guid id, [FromBody] ReplyToTicketDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { success = false, message = "Usuário não identificado." });

                var ticket = await _service.GetTicketByIdAsync(id);
                if (ticket.UserId != userId)
                    return Forbid();

                await _service.ReplyToTicketAsync(id, userId, "Student", dto);
                return Ok(new { success = true, message = "Resposta enviada com sucesso." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao enviar resposta.");
            }
        }
    }
}
