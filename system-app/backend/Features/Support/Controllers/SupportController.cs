using System.Security.Claims;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Support.DTOs;
using MeuCrudCsharp.Features.Support.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Support.Controllers
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
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(
                        new { success = false, message = "Usuário não identificado." }
                    );

                await _service.CreateTicketAsync(userId, dto);
                return Created(
                    "",
                    new { success = true, message = "Ticket de suporte criado com sucesso." }
                );
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao criar ticket.");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(string id)
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(
            string id,
            [FromBody] UpdateSupportTicketDto dto
        )
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
    }
}
