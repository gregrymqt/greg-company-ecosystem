using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Controllers;

[Route("api/[controller]")]
public class PixController : MercadoPagoApiControllerBase
{
    private readonly IPixPaymentService _paymentService;

    public PixController(IPixPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("createpix")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePixPayment([FromBody] CreatePixPaymentRequest request)
    {
        if (
            !Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey)
            || string.IsNullOrEmpty(idempotencyKey)
        )
        {
            return BadRequest(new { message = "O header 'X-Idempotency-Key' é obrigatório." });
        }

        try
        {
            var response = await _paymentService.CreateIdempotentPixPaymentAsync(
                request,
                idempotencyKey.ToString()
            );

            return StatusCode(response.StatusCode, response.Body);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao criar pagamento PIX.");
        }
    }
}
