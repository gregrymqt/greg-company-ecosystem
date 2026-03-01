using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Refunds.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Refunds.Controllers
{
    [Route("api/[controller]")]
    public class RefundsController : MercadoPagoApiControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundsController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [HttpPost("{paymentId}")]
        public async Task<IActionResult> RequestRefund([FromRoute] string paymentId)
        {
            try
            {
                if (!long.TryParse(paymentId, out var paymentIdLong))
                {
                    return BadRequest(
                        new { success = false, message = "O ID do pagamento deve ser numérico." }
                    );
                }

                await _refundService.RequestRefundAsync(paymentIdLong);

                return Ok(new { success = true, message = "Reembolso solicitado com sucesso." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Não foi possível processar o reembolso.");
            }
        }
    }
}
