using System.Text.Json;
using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Services
{
    public class MercadoPagoPaymentService(
        IHttpClientFactory httpClient,
        ILogger<MercadoPagoPaymentService> logger)
        : MercadoPagoServiceBase(httpClient, logger), IMercadoPagoPaymentService
    {
        public async Task<MercadoPagoPaymentDetails?> GetPaymentStatusAsync(
            string externalPaymentId
        )
        {
            logger.LogInformation(
                "Buscando status do pagamento {PaymentId} no Mercado Pago.",
                externalPaymentId
            );

            var endpoint = $"/v1/payments/{externalPaymentId}";

            var responseJson = await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Get,
                endpoint,
                payload: null
            );

            if (string.IsNullOrEmpty(responseJson))
            {
                return null;
            }

            var paymentDetails = JsonSerializer.Deserialize<MercadoPagoPaymentDetails>(
                responseJson
            );

            return paymentDetails;
        }
    }
}
