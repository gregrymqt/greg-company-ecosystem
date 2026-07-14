using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Interfaces;
using MercadoPago.Client;
using MercadoPago.Client.Preference;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Payments.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Application.Services;

public class PreferencePaymentService(
    ILogger<PreferencePaymentService> logger,
    IOptions<GeneralSettings> settings,
    IUserContext userContext,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Interfaces.IPlanRepository planRepository,
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

        Plan? plan = null;
        if (Guid.TryParse(model.PlanId, out var publicId))
        {
            plan = await planRepository.GetByPublicIdAsync(publicId, true);
        }
        if (plan == null)
        {
            plan = await planRepository.GetActiveByExternalIdAsync(model.PlanId);
        }

        if (plan == null)
            throw new ArgumentException("Plano não encontrado.");

        var amount = plan.TransactionAmount;
        var title = plan.Name;
        var description = plan.Description ?? plan.Name;

        if (amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");

        var externalReference = Guid.NewGuid().ToString();

        try
        {
            var initialPayment = new MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments
            {
                UserId = user.Id,
                Status = "pending",
                Amount = amount,
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
                        Id = model.PlanId,
                        Title = title,
                        Description = description,
                        Quantity = 1,
                        UnitPrice = amount,
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
