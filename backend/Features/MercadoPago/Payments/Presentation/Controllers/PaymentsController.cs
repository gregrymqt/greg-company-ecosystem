using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Payments;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : MercadoPagoApiControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IUserContext _userContext;

    public PaymentsController(IPaymentService paymentService, IUserContext userContext)
    {
        _paymentService = paymentService;
        _userContext = userContext;
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory()
    {
        try
        {
            var userId = _userContext.GetCurrentUserId().ToString();
            var history = await _paymentService.GetUserPaymentHistoryAsync(userId);

            return Ok(history);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao recuperar histórico de pagamentos.");
        }
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] string? search = null)
    {
        try
        {
            var response = await _paymentService.GetAdminPaymentsPaginatedAsync(page, pageSize, status, search);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao recuperar pagamentos (Admin).");
        }
    }
}
