using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Controllers
{
    [Route("api/subscriptions")]
    public class SubscriptionsController : MercadoPagoApiControllerBase
    {
        private readonly IUserSubscriptionService _service;

        public SubscriptionsController(IUserSubscriptionService service)
        {
            _service = service;
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetDetails()
        {
            try
            {
                var result = await _service.GetMySubscriptionDetailsAsync();

                if (result == null)
                {
                    return NotFound(
                        new { success = false, message = "Nenhuma assinatura encontrada." }
                    );
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar detalhes da assinatura.");
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] UpdateSubscriptionStatusDto request
        )
        {
            try
            {
                await _service.ChangeSubscriptionStatusAsync(request.Status);

                return Ok(
                    new { success = true, message = "Status da assinatura atualizado com sucesso." }
                );
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Não foi possível atualizar o status da assinatura.");
            }
        }
    }
}
