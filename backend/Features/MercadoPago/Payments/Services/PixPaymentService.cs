using MercadoPago.Client;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Error;

using MeuCrudCsharp.Features.Auth.Interfaces;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Caching.Record;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Hub;
using MeuCrudCsharp.Features.MercadoPago.Notification.Record;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Utils;
using MeuCrudCsharp.Features.Shared.Work;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Services;

public class PixPaymentService(
    ILogger<PixPaymentService> logger,
    ICacheService cacheService,
    IPaymentNotificationHub notificationHub,
    IPaymentRepository paymentRepository, // Injeção do Repo
    IUnitOfWork unitOfWork, // Injeção do UoW
    IOptions<GeneralSettings> settings,
    IUserContext userContext)
    : IPixPaymentService
{
    private readonly GeneralSettings _generalsettings = settings.Value;
    private const string IDEMPOTENCY_PREFIX = "PixPayment";

    public async Task<CachedResponse> CreateIdempotentPixPaymentAsync(
        CreatePixPaymentRequest request,
        string idempotencyKey
    )
    {
        var cacheKey = $"{IDEMPOTENCY_PREFIX}_idempotency_pix_{idempotencyKey}";

        var response = await cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    var result = await CreatePixPaymentAsync(
                        await userContext.GetCurrentUserId(),
                        request,
                        idempotencyKey
                    );

                    return new CachedResponse(result, 200);
                }
                catch (MercadoPagoApiException mpex)
                {
                    var errorBody = new
                    {
                        error = "MercadoPago Error",
                        message = mpex.ApiError?.Message ?? "Erro ao comunicar com o provedor.",
                    };
                    logger.LogError(
                        mpex,
                        "Erro da API do Mercado Pago (IdempotencyKey: {Key}): {ApiError}",
                        idempotencyKey,
                        mpex.ApiError?.Message
                    );
                    return new CachedResponse(errorBody, 400);
                }
                catch (Exception ex)
                {
                    var errorBody = new
                    {
                        message = "Ocorreu um erro inesperado.",
                        error = ex.Message,
                    };
                    logger.LogError(
                        ex,
                        "Erro inesperado ao processar PIX (IdempotencyKey: {Key})",
                        idempotencyKey
                    );
                    return new CachedResponse(errorBody, 500);
                }
            },
            TimeSpan.FromHours(24)
        );

        return response ?? throw new InvalidOperationException();
    }

    private async Task<PaymentResponseDto> CreatePixPaymentAsync(
        string userId,
        CreatePixPaymentRequest request,
        string externalReference
    )
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("userId is required");
        }

        try
        {
            await notificationHub.SendStatusUpdateAsync(
                userId,
                new PaymentStatusUpdate("A processar o seu pagamento...", "processing", false)
            );

            var novoPixPayment = new Models.Payments()
            {
                UserId = userId,
                Status = "Iniciando",
                PayerEmail = request.Payer.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExternalId = externalReference,
                Amount = request.TransactionAmount,
                Method = "Pix",
                CustomerCpf = request.Payer.Identification.Number,
            };

            await paymentRepository.AddAsync(novoPixPayment);

            logger.LogInformation(
                "Pix payment preparado para o usuário {UserId}. Aguardando resposta do MercadoPago.",
                userId
            );

            await notificationHub.SendStatusUpdateAsync(
                userId,
                new PaymentStatusUpdate(
                    "Comunicando com o provedor de pagamento...",
                    "processing",
                    false
                )
            );

            var requestOptions = new RequestOptions
            {
                CustomHeaders = { { "X-Idempotency-Key", externalReference } },
            };

            var paymentClient = new PaymentClient();
            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = request.TransactionAmount,
                Description = request.Description,
                PaymentMethodId = "pix",
                Payer = new PaymentPayerRequest
                {
                    Email = request.Payer.Email,
                    FirstName = request.Payer.FirstName,
                    LastName = request.Payer.LastName,
                    Identification = new IdentificationRequest
                    {
                        Type = request.Payer.Identification.Type,
                        Number = request.Payer.Identification.Number,
                    },
                },
                ExternalReference = externalReference,
                NotificationUrl = $"{_generalsettings.BaseUrl}/webhook/mercadpago",
            };

            var payment = await paymentClient.CreateAsync(paymentRequest, requestOptions);

            novoPixPayment.PaymentId = payment.Id.ToString();
            novoPixPayment.Status = PaymentStatusMapper.MapFromMercadoPago(payment.Status);
            novoPixPayment.DateApproved = payment.DateApproved;
            novoPixPayment.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Pagamento PIX {PaymentId} para o usuário {UserId} salvo com sucesso. Status: {Status}",
                novoPixPayment.PaymentId,
                userId,
                novoPixPayment.Status
            );

            if (
                payment.Status == "approved"
                || payment.Status == "pending"
                || payment.Status == "in_process"
            )
            {
                await notificationHub.SendStatusUpdateAsync(
                    userId,
                    new PaymentStatusUpdate(
                        "Pagamento processado com sucesso!",
                        "approved",
                        true,
                        payment.Id.ToString()
                    )
                );
            }
            else
            {
                await notificationHub.SendStatusUpdateAsync(
                    userId,
                    new PaymentStatusUpdate(
                        payment.StatusDetail ?? "O pagamento foi recusado.",
                        "failed",
                        true,
                        payment.Id.ToString()
                    )
                );
            }

            return new PaymentResponseDto(
                payment.Status,
                payment.Id.GetValueOrDefault(), // Safe unwrap
                null,
                "Pagamento PIX criado com sucesso.",
                payment.PointOfInteraction?.TransactionData?.QrCode,
                payment.PointOfInteraction?.TransactionData?.QrCodeBase64
            );
        }
        catch (Exception ex)
        {
            var mensagemErro = "Ocorreu um erro inesperado em nosso sistema.";

            if (ex is MercadoPagoApiException mpex)
            {
                mensagemErro = mpex.ApiError?.Message ?? "Erro ao comunicar com o provedor.";
                logger.LogError(
                    mpex,
                    "Erro da API do Mercado Pago ao processar PIX para {UserId}: {ApiError}",
                    userId,
                    mpex.ApiError?.Message
                );
            }
            else
            {
                logger.LogError(
                    ex,
                    "Erro inesperado ao processar pagamento PIX para {UserId}.",
                    userId
                );
            }

            logger.LogWarning(
                "Pagamento PIX para o usuário {UserId} NÃO foi persistido devido a uma falha. Rollback automático.",
                userId
            );

            await notificationHub.SendStatusUpdateAsync(
                userId,
                new PaymentStatusUpdate(mensagemErro, "error", true)
            );

            throw new AppServiceException(mensagemErro, ex);
        }
    }
}
