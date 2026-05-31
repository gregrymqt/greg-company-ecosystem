using MeuCrudCsharp.Features.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Presentation.Controllers;

[Route("api/[controller]")]
public class ConfiguracaoController : ApiControllerBase
{
    private readonly MercadoPagoSettings _mercadoPagoSettings;

    public ConfiguracaoController(IOptions<MercadoPagoSettings> mercadoPagoSettings)
    {
        _mercadoPagoSettings = mercadoPagoSettings.Value;
    }

    [HttpGet("public-key")]
    public Task<IActionResult> GetPublicKey()
    {
        return Task.FromResult((IActionResult)Ok(new { publicKey = _mercadoPagoSettings.PublicKey }));
    }
}
    