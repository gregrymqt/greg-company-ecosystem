namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Services;

using Auth.Interfaces;
using Caching.Interfaces;
using Exceptions;
using MercadoPago.Subscriptions.DTOs;
using MercadoPago.Subscriptions.Interfaces;
using Models.Enums;
using Shared.Work;

public class UserSubscriptionService : IUserSubscriptionService
{
    private readonly IUserContext _userContext;
    private readonly ISubscriptionRepository _repository;
    private readonly IMercadoPagoSubscriptionService _mpSubscriptionService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserSubscriptionService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UserSubscriptionService(
        IUserContext userContext,
        ISubscriptionRepository repository,
        IMercadoPagoSubscriptionService mpSubscriptionService,
        ICacheService cacheService,
        ILogger<UserSubscriptionService> logger,
        IUnitOfWork unitOfWork
    )
    {
        _userContext = userContext;
        _repository = repository;
        _mpSubscriptionService = mpSubscriptionService;
        _cacheService = cacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDetailsDto?> GetMySubscriptionDetailsAsync()
    {
        var userId = await _userContext.GetCurrentUserId();

        return await _cacheService.GetOrCreateAsync(
            $"SubscriptionDetails_{userId}",
            async () =>
            {


                var mpSubscription = await _mpSubscriptionService.GetSubscriptionByIdAsync(
                    subscription.ExternalId ?? string.Empty
                );

                if (mpSubscription == null)
                    return null;

                return new SubscriptionDetailsDto(
                    subscription.ExternalId,
                    subscription.Plan.Name,
                    mpSubscription.Status,
                    (decimal)subscription.CurrentAmount,
                    subscription.LastFourCardDigits,
                    mpSubscription.NextPaymentDate
                );
            }
        );
    }

    public async Task ChangeSubscriptionStatusAsync(string newStatus)
    {
        var userId = await _userContext.GetCurrentUserId();

        var statusEnum = SubscriptionStatusExtensions.FromMpString(newStatus);
        if (statusEnum == SubscriptionStatus.Unknown)
        {
            throw new AppServiceException($"Status '{newStatus}' inválido.");
        }

        var subscription = await _repository.GetByExternalIdAsync(
            await GetActiveSubscriptionExternalIdAsync(userId),
            includePlan: false,
            asNoTracking: false
        ) ?? throw new ResourceNotFoundException(
            "Nenhuma assinatura ativa encontrada para atualização."
        );

        var originalStatus = subscription.Status;

        subscription.Status = statusEnum.ToMpString();
        subscription.UpdatedAt = DateTime.UtcNow;

        try
        {
            var dto = new UpdateSubscriptionStatusDto(statusEnum.ToMpString());
            var result = await _mpSubscriptionService.UpdateSubscriptionStatusAsync(
                subscription.ExternalId ?? string.Empty,
                dto
            );

            if (result.Status != statusEnum.ToMpString())
            {
                _logger.LogWarning(
                    "MP retornou status {MpStatus} diferente do solicitado {RequestedStatus}",
                    result.Status,
                    statusEnum.ToMpString()
                );
                subscription.Status = result.Status;
            }

            await _unitOfWork.CommitAsync();

            await _cacheService.RemoveAsync($"SubscriptionDetails_{userId}");

            _logger.LogInformation(
                "Assinatura {Id} do usuário {UserId} atualizada para {Status}.",
                subscription.ExternalId,
                userId,
                subscription.Status
            );
        }
        catch (ExternalApiException ex)
        {
            _logger.LogError(
                ex,
                "Falha no MP ao atualizar status da assinatura para {NewStatus}. Rollback automático.",
                newStatus
            );

            subscription.Status = originalStatus;

            _logger.LogInformation(
                "Rollback concluído. Status da assinatura permanece em {OriginalStatus}",
                originalStatus
            );

            throw;
        }
    }

    private async Task<string> GetActiveSubscriptionExternalIdAsync(string userId)
    {
        var subscription = await _repository.GetActiveSubscriptionByUserIdAsync(userId);
        return subscription?.ExternalId 
            ?? throw new ResourceNotFoundException("Nenhuma assinatura ativa encontrada.");
    }
}
