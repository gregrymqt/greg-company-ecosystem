// Service for processing refunds via Mercado Pago.
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Services
{
    public class RefundService(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClient,
        ILogger<RefundService> logger
    ) : MercadoPagoServiceBase(httpClient, logger), IRefundService
    {
        public async Task RequestRefundAsync(long paymentId)
        {
            var externalIdStr = paymentId.ToString();

            var payment = await paymentRepository.GetByExternalIdWithSubscriptionAsync(
                externalIdStr
            );

            if (payment == null)
            {
                throw new ResourceNotFoundException(
                    $"Pagamento {paymentId} nï¿½o encontrado no sistema."
                );
            }

            if (payment.Status != "aprovada" && payment.Status != "approved")
            {
                throw new AppServiceException(
                    "Apenas pagamentos aprovados podem ser reembolsados."
                );
            }

            if (payment.CreatedAt < DateTime.UtcNow.AddDays(-7))
            {
                throw new AppServiceException(
                    "O prazo de 7 dias para solicitaï¿½ï¿½o de reembolso expirou."
                );
            }

            try
            {
                await RefundPaymentOnMercadoPagoAsync(payment.ExternalId);

                payment.Status = "refunded";
                payment.UpdatedAt = DateTime.UtcNow;

                paymentRepository.Update(payment);

                if (payment.Subscription != null)
                {
                    payment.Subscription.Status = "refund_pending";
                    payment.Subscription.UpdatedAt = DateTime.UtcNow;

                    subscriptionRepository.Update(payment.Subscription);
                }

                await unitOfWork.CommitAsync();

                logger.LogInformation(
                    "Reembolso solicitado com sucesso para o pagamento {ExternalId}.",
                    payment.ExternalId
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Falha ao processar reembolso no Mercado Pago para o pagamento {ExternalId}.",
                    payment.ExternalId
                );
                throw;
            }
        }

        private async Task RefundPaymentOnMercadoPagoAsync(
            string externalPaymentId,
            decimal? amount = null
        )
        {
            logger.LogInformation(
                "Iniciando request de reembolso MP para: {PaymentId}",
                externalPaymentId
            );

            var endpoint = $"/v1/payments/{externalPaymentId}/refunds";
            var payload = new { amount = amount };

            await SendMercadoPagoRequestAsync(HttpMethod.Post, endpoint, payload);
        }
    }
}

