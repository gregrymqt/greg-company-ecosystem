using MercadoPago.Client;
using MercadoPago.Client.Preference;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Features.Auth.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Services;

public class PreferencePaymentService(
    ILogger<PreferencePaymentService> logger,
    IOptions<GeneralSettings> settings,
    IUserContext userContext,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository)
    : IPreferencePaymentService
{
    private readonly GeneralSettings _generalSettings = settings.Value;


    public async Task<string> CreatePreferenceAsync(CreatePreferenceDto model)
    {
        var userId = await userContext.GetCurrentUserId();
        var user = await userRepository.GetByIdAsync(userId);
        
        if (userId == null)
            throw new UnauthorizedAccessException("Usuário não encontrado.");

        if (model.Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");

        var externalReference = Guid.NewGuid().ToString();

        try
        {
            var initialPayment = new Models.Payments
            {
                UserId = user.Id,
                Status = "pending",
                Amount = model.Amount,
                Method = "preference_checkout",
                ExternalId = externalReference,
                PayerEmail = user.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await paymentRepository.AddAsync(initialPayment);

            var requestOptions = new RequestOptions();
            requestOptions.CustomHeaders.Add("x-idempotency-key", Guid.NewGuid().ToString());

            var baseUrl = _generalSettings.BaseUrl;

            var preferenceRequest = new PreferenceRequest
            {
                Items =
                [
                    new PreferenceItemRequest
                    {
                        Id = "CURSO-V1",
                        Title = model.Title,
                        Description = model.Description,
                        Quantity = 1,
                        UnitPrice = model.Amount,
                        CurrencyId = "BRL",
                    },
                ],
                Payer = new PreferencePayerRequest { Name = user.Name, Email = user.Email },
                Purpose = "wallet_purchase",
                ExternalReference = externalReference,
                NotificationUrl = $"{baseUrl}/webhook/mercadopago",
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{baseUrl}/pagamento/success",
                    Failure = $"{baseUrl}/pagamento/error",
                    Pending = $"{baseUrl}/pagamento/pending",
                },
                AutoReturn = "approved",
            };

            var client = new PreferenceClient();
            var preference = await client.CreateAsync(preferenceRequest, requestOptions);

            initialPayment.PaymentId = preference.Id;
            initialPayment.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "Preferência criada com sucesso: {PrefId} | ExternalRef: {Ref} | UserId: {UserId}",
                preference.Id,
                externalReference,
                userId
            );

            return preference.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao criar preferência de pagamento para o usuário {UserId}.",
                userId
            );

            throw new AppServiceException("Erro ao gerar link de pagamento.", ex);
        }
    }
}
