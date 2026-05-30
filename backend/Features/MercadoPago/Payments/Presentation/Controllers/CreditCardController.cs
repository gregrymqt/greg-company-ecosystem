using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Presentation.Controllers
{
    [Route("/api/credit/card")]
    public class CreditCardController : MercadoPagoApiControllerBase
    {
        private readonly ICreditCardPaymentService _creditCardPaymentService;

        public CreditCardController(ICreditCardPaymentService creditCardPaymentService)
        {
            _creditCardPaymentService = creditCardPaymentService;
        }

        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPaymentAsync(
            [FromBody] CreditCardPaymentRequestDto request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
            {
                return BadRequest(new { message = "O header 'X-Idempotency-Key' é obrigatório." });
            }

            var response = await _creditCardPaymentService.CreatePaymentOrSubscriptionAsync(
                request,
                idempotencyKey.ToString()
            );

            if (response.StatusCode == 201)
            {
                return CreatedAtAction(nameof(ProcessPaymentAsync), response.Body);
            }

            return StatusCode(response.StatusCode, response.Body);
        }
    }
}
