using MeuCrudCsharp.Features.Base;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Shared.Token;

[ApiController]
[Route("api/antiforgery")]
public class AntiforgeryController : ApiControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    [HttpGet("token")]
    [IgnoreAntiforgeryToken]
    public IActionResult GetToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(
            new
            {
                token = tokens.RequestToken,
                headerName = tokens.HeaderName,
            }
        );
    }
}
