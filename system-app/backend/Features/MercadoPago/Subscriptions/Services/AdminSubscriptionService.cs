using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Plans.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILogger<SubscriptionService> _logger;
        private readonly ICacheService _cacheService;
        private readonly IMercadoPagoSubscriptionService _mpSubscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IPlanRepository _planRepository;
        private readonly GeneralSettings _generalSettings;
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionService(
            ILogger<SubscriptionService> logger,
            ICacheService cacheService,
            IMercadoPagoSubscriptionService mpSubscriptionService,
            ISubscriptionRepository subscriptionRepository,
            IPlanRepository planRepository,
            IOptions<GeneralSettings> generalSettings,
            IUnitOfWork unitOfWork
        )
        {
            _logger = logger;
            _cacheService = cacheService;
            _mpSubscriptionService = mpSubscriptionService;
            _subscriptionRepository = subscriptionRepository;
            _planRepository = planRepository;
            _generalSettings = generalSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<Subscription> CreateSubscriptionAsync(
            string userId,
            string planExternalId,
            string savedCardId,
            string payerEmail,
            string lastFourDigits
        )
        {
            var localPlan =
                await _planRepository.GetActiveByExternalIdAsync(planExternalId)
                ?? throw new ResourceNotFoundException(
                    $"Plano com ID externo '{planExternalId}' não encontrado."
                );

            var newSubscription = new Subscription
            {
                UserId = userId,
                Status = SubscriptionStatus.Pending.ToMpString(),
                ExternalId = planExternalId,
                CardTokenId = savedCardId,
                PayerEmail = payerEmail,
                PlanId = localPlan.Id,
                CreatedAt = DateTime.UtcNow,
                LastFourCardDigits = lastFourDigits,
                CurrentAmount = (int)localPlan.TransactionAmount,
                CurrentPeriodStartDate = DateTime.UtcNow,
                CurrentPeriodEndDate = DateTime.UtcNow.AddMonths(localPlan.FrequencyInterval),
                PayerMpId = "pending_mp_id",
                PaymentMethodId = "credit_card",
            };

            await _subscriptionRepository.AddAsync(newSubscription);

            _logger.LogInformation(
                "Assinatura preparada para o usuário {UserId}. Aguardando resposta do MercadoPago.",
                userId
            );

            var periodStartDate = DateTime.UtcNow;
            var periodEndDate = periodStartDate.AddMonths(localPlan.FrequencyInterval);
            var frequencyType =
                localPlan.FrequencyType == PlanFrequencyType.Months ? "months" : "days";

            var payloadForMp = new CreateSubscriptionDto(
                planExternalId,
                localPlan.Name,
                payerEmail,
                savedCardId,
                $"{_generalSettings.BaseUrl}/Profile/User/Index",
                new AutoRecurringDto(
                    localPlan.FrequencyInterval,
                    frequencyType,
                    localPlan.TransactionAmount,
                    "BRL",
                    periodStartDate,
                    periodEndDate
                ),
                SubscriptionStatus.Authorized.ToMpString(),
                userId
            );

            try
            {
                var subscriptionResponse = await _mpSubscriptionService.CreateSubscriptionAsync(
                    payloadForMp
                );

                _logger.LogInformation(
                    "Assinatura criada no MP com ID {ExternalId}",
                    subscriptionResponse.Id
                );

                newSubscription.ExternalId = subscriptionResponse.Id;
                newSubscription.PayerMpId = subscriptionResponse.PayerId.ToString();
                newSubscription.PaymentMethodId = subscriptionResponse.PaymentMethodId;
                newSubscription.CurrentPeriodStartDate = subscriptionResponse
                    .AutoRecurring
                    .StartDate;
                newSubscription.CurrentPeriodEndDate = subscriptionResponse.AutoRecurring.EndDate;
                newSubscription.Status = subscriptionResponse.Status;

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Assinatura {SubscriptionId} para o usuário {UserId} salva com sucesso. Status: {Status}",
                    newSubscription.ExternalId,
                    userId,
                    newSubscription.Status
                );

                return newSubscription;
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(
                    ex,
                    "Falha no MP ao criar assinatura para o usuário {UserId}. Rollback automático.",
                    userId
                );

                _logger.LogInformation(
                    "Rollback automático concluído. Assinatura para o usuário {UserId} NÃO foi persistida.",
                    userId
                );

                throw;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(
                    dbEx,
                    "Erro ao salvar assinatura local após sucesso no MP. Cancelando no MP..."
                );

                if (string.IsNullOrEmpty(newSubscription.ExternalId) ||
                    newSubscription.ExternalId == planExternalId)
                    throw new AppServiceException(
                        "Erro de consistência de dados. A operação foi revertida.",
                        dbEx
                    );
                try
                {
                    await _mpSubscriptionService.CancelSubscriptionAsync(newSubscription.ExternalId);
                    _logger.LogInformation(
                        "Assinatura {ExternalId} cancelada no MP devido a erro de persistência local.",
                        newSubscription.ExternalId
                    );
                }
                catch (Exception cancelEx)
                {
                    _logger.LogError(
                        cancelEx,
                        "Falha ao cancelar assinatura {ExternalId} no MP. Intervenção manual necessária!",
                        newSubscription.ExternalId
                    );
                }

                throw new AppServiceException(
                    "Erro de consistência de dados. A operação foi revertida.",
                    dbEx
                );
            }
        }

        public async Task<SubscriptionResponseDto> UpdateSubscriptionValueAsync(
            string subscriptionId,
            UpdateSubscriptionValueDto dto
        )
        {
            _logger.LogInformation(
                "Atualizando valor da assinatura: {SubscriptionId}",
                subscriptionId
            );

            var localSubscription =
                await _subscriptionRepository.GetByExternalIdAsync(
                    subscriptionId,
                    includePlan: false,
                    asNoTracking: false
                )
                ?? throw new ResourceNotFoundException(
                    $"Assinatura {subscriptionId} não encontrada."
                );

            var originalAmount = localSubscription.CurrentAmount;

            if (originalAmount == dto.TransactionAmount)
            {
                return await _mpSubscriptionService.GetSubscriptionByIdAsync(subscriptionId) ??
                       throw new InvalidOperationException();
            }

            localSubscription.CurrentAmount = (int)dto.TransactionAmount;

            try
            {
                var mpResponse = await _mpSubscriptionService.UpdateSubscriptionValueAsync(
                    subscriptionId,
                    dto
                );

                await _unitOfWork.CommitAsync();

                await _cacheService.RemoveAsync($"SubscriptionDetails_{localSubscription.UserId}");

                _logger.LogInformation(
                    "Valor da assinatura {SubscriptionId} atualizado com sucesso. Novo valor: {NewAmount}",
                    subscriptionId,
                    dto.TransactionAmount
                );

                return mpResponse;
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(
                    ex,
                    "Falha no MP ao atualizar valor da assinatura {SubscriptionId}. Rollback automático.",
                    subscriptionId
                );

                localSubscription.CurrentAmount = originalAmount;

                _logger.LogInformation(
                    "Rollback concluído. Valor da assinatura {SubscriptionId} permanece em {OriginalAmount}",
                    subscriptionId,
                    originalAmount
                );

                throw;
            }
        }

        public async Task<SubscriptionResponseDto> UpdateSubscriptionStatusAsync(
            string subscriptionId,
            UpdateSubscriptionStatusDto dto
        )
        {
            var localSubscription =
                await _subscriptionRepository.GetByExternalIdAsync(
                    subscriptionId,
                    asNoTracking: false
                ) ?? throw new ResourceNotFoundException("Assinatura não encontrada.");

            var originalStatus = localSubscription.Status;

            localSubscription.Status = dto.Status;
            localSubscription.UpdatedAt = DateTime.UtcNow;

            try
            {
                var mpResponse = await _mpSubscriptionService.UpdateSubscriptionStatusAsync(
                    subscriptionId,
                    dto
                );

                await _unitOfWork.CommitAsync();

                await _cacheService.RemoveAsync($"SubscriptionDetails_{localSubscription.UserId}");

                _logger.LogInformation(
                    "Status da assinatura {SubscriptionId} atualizado com sucesso. Novo status: {NewStatus}",
                    subscriptionId,
                    dto.Status
                );

                return mpResponse;
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(
                    ex,
                    "Falha no MP ao atualizar status da assinatura {SubscriptionId}. Rollback automático.",
                    subscriptionId
                );

                localSubscription.Status = originalStatus;

                _logger.LogInformation(
                    "Rollback concluído. Status da assinatura {SubscriptionId} permanece em {OriginalStatus}",
                    subscriptionId,
                    originalStatus
                );

                throw;
            }
        }

        public async Task<SubscriptionResponseDto> GetSubscriptionByIdAsync(string subscriptionId)
        {
            return await _mpSubscriptionService.GetSubscriptionByIdAsync(subscriptionId) ??
                   throw new InvalidOperationException();
        }

        public async Task<Subscription> ActivateSubscriptionFromSinglePaymentAsync(
            string userId,
            Guid planPublicId,
            string paymentId,
            string payerEmail,
            string? lastFourCardDigits
        )
        {
            var localPlan = await _planRepository.GetByPublicIdAsync(planPublicId, true);
            if (localPlan == null)
                throw new ResourceNotFoundException("Plano não encontrado.");

            var now = DateTime.UtcNow;
            var expirationDate = now.AddMonths(localPlan.FrequencyInterval);

            var newSubscription = new Subscription
            {
                UserId = userId,
                PlanId = localPlan.Id,
                ExternalId = paymentId,
                Status = SubscriptionStatus.Authorized.ToMpString(),
                PayerEmail = payerEmail,
                PaymentId = paymentId,
                CreatedAt = now,
                CurrentPeriodStartDate = now,
                CurrentPeriodEndDate = expirationDate,
                LastFourCardDigits = lastFourCardDigits,
                CurrentAmount = (int)localPlan.TransactionAmount,
                PayerMpId = "single_payment",
                PaymentMethodId = "pix_or_ticket",
            };

            await _subscriptionRepository.AddAsync(newSubscription);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation(
                "Assinatura ativada a partir de pagamento único. UserId: {UserId}, PaymentId: {PaymentId}",
                userId,
                paymentId
            );

            return newSubscription;
        }
    }
}