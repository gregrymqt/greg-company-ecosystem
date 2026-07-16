using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Presentation.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/subscriptions")]
    public class AdminSubscriptionsController : MercadoPagoApiControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public AdminSubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? statusFilter = null, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (items, totalCount) = await _subscriptionService.GetPaginatedSubscriptionsAsync(page, pageSize, statusFilter, searchTerm);
                return Ok(new { data = items, totalCount, page, pageSize });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar assinaturas.");
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try
            {
                var result = await _subscriptionService.GetSubscriptionByIdAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar assinatura.");
            }
        }

        [HttpPut("{id}/value")]
        [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateValue(
            string id,
            [FromBody] UpdateSubscriptionValueDto dto
        )
        {
            try
            {
                var result = await _subscriptionService.UpdateSubscriptionValueAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao atualizar valor da assinatura.");
            }
        }

        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatus(
            string id,
            [FromBody] UpdateSubscriptionStatusDto dto
        )
        {
            try
            {
                var result = await _subscriptionService.UpdateSubscriptionStatusAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao atualizar status da assinatura.");
            }
        }
    }
}
