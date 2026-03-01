using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Webhooks.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.MercadoPago.Webhooks.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebHookController : MercadoPagoApiControllerBase
    {
        private readonly ILogger<WebHookController> _logger;
        private readonly IWebhookService _webhookService;

        public WebHookController(ILogger<WebHookController> logger, IWebhookService webhookService)
        {
            _logger = logger;
            _webhookService = webhookService;
        }

        [HttpPost("mercadopago")]
        public async Task<IActionResult> MercadoPagoWebhook(
            [FromBody] MercadoPagoWebhookNotification notification
        )
        {
            _logger.LogInformation(
                "Webhook recebido: Tipo={Type}, Action={Action}, ID={Id}",
                notification.Type,
                notification.Action,
                notification.Data.ToString()
            );

            try
            {
                if (!_webhookService.IsSignatureValid(Request, notification))
                {
                    return BadRequest(new { error = "Assinatura inválida." });
                }

                await _webhookService.ProcessWebhookNotificationAsync(notification);

                return Accepted(new { status = "received" });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro no endpoint de Webhook.");
            }
        }
    }
}
