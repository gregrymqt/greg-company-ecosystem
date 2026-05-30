using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.MercadoPago.Plans.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Controllers
{
    [Route("api/public/plans")]
    public class PublicPlansController : MercadoPagoApiControllerBase
    {
        private readonly IPlanService _planService;

        public PublicPlansController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlans(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var plans = await _planService.GetActiveDbPlansAsync(page, pageSize);
                return Ok(plans);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "An error occurred while fetching public plans.");
            }
        }
    }
}
