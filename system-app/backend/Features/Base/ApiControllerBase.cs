using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Base;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public abstract class ApiControllerBase : ControllerBase
{
    protected virtual IActionResult HandleException(Exception ex, string friendlyMessage)
    {
        switch (ex)
        {
            case AppServiceException:
            case InvalidOperationException:
                return BadRequest(
                    new
                    {
                        success = false,
                        message = friendlyMessage,
                        error = ex.Message,
                    }
                );
            case ResourceNotFoundException:
                return NotFound(new { success = false, message = ex.Message });
            default:
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = friendlyMessage,
                        details = "Ocorreu um erro interno no servidor. Tente novamente mais tarde.",
                    }
                );
        }
    }
}

[RateLimit(5, 60)]
public abstract class MercadoPagoApiControllerBase : ApiControllerBase
{
    protected override IActionResult HandleException(Exception ex, string friendlyMessage)
    {
        switch (ex)
        {
            case AppServiceException:
            case InvalidOperationException:
                return BadRequest(
                    new
                    {
                        success = false,
                        message = friendlyMessage,
                        error = ex.Message,
                    }
                );
            case ResourceNotFoundException:
                return NotFound(new { success = false, message = ex.Message });
            default:
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = friendlyMessage,
                        details = "Ocorreu um erro interno no servidor. Tente novamente mais tarde.",
                    }
                );
        }
    }
}
