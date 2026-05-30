using MeuCrudCsharp.Features.Auth.Interfaces;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Clients.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Clients.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;
using Microsoft.Extensions.Logging;

namespace MeuCrudCsharp.Features.MercadoPago.Clients.Services;

public class ClientService(
    IClientMercadoPagoService mpService,
    ICacheService cacheService,
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    ILogger<ClientService> logger)
    : IClientService
{

    public async Task<List<WalletCardDto>> GetUserWalletAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ResourceNotFoundException("Usuário não encontrado.");

        if (string.IsNullOrEmpty(user.CustomerId))
            return [];

        var mpCards = await ListCardsFromCustomerAsync(user.CustomerId);

        var activeSubscription = await subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);

        return mpCards
            .Select(card => new WalletCardDto
            {
                Id = card.Id ?? "",
                LastFourDigits = card.LastFourDigits ?? "****",
                ExpirationMonth = card.ExpirationMonth ?? 0,
                ExpirationYear = card.ExpirationYear ?? 0,
                PaymentMethodId = card.PaymentMethod?.Id ?? "unknown",
                IsSubscriptionActiveCard =
                    activeSubscription != null && activeSubscription.CardTokenId == card.Id,
            })
            .ToList();
    }

    public async Task<WalletCardDto> AddCardToWalletAsync(string userId, string cardToken)
    {
        if (string.IsNullOrWhiteSpace(cardToken))
            throw new ArgumentException("Token do cartão não pode ser vazio.", nameof(cardToken));

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ResourceNotFoundException("Usuário não encontrado.");

        try
        {
            CardInCustomerResponseDto resultCard;
            if (string.IsNullOrEmpty(user.CustomerId))
            {
                logger.LogInformation("Usuário {UserId} não tem CustomerId. Criando Customer no MP.", userId);

                var newCustomer = await mpService.CreateCustomerAsync(user.Email!, user.Name!);

                user.CustomerId = newCustomer.Id;
                userRepository.Update(user);

                resultCard = await AddCardToCustomerAsync(newCustomer.Id!, cardToken);

                await unitOfWork.CommitAsync();

                logger.LogInformation("Customer {CustomerId} criado e cartão adicionado para usuário {UserId}.", 
                    newCustomer.Id, userId);
            }
            else
            {
                resultCard = await AddCardToCustomerAsync(user.CustomerId, cardToken);
                
                logger.LogInformation("Cartão adicionado ao Customer {CustomerId}.", user.CustomerId);
            }

            return new WalletCardDto
            {
                Id = resultCard.Id ?? "",
                LastFourDigits = resultCard.LastFourDigits ?? "****",
                ExpirationMonth = resultCard.ExpirationMonth ?? 0,
                ExpirationYear = resultCard.ExpirationYear ?? 0,
                PaymentMethodId = resultCard.PaymentMethod?.Id ?? "unknown",
                IsSubscriptionActiveCard = false,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao adicionar cartão para usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<CustomerWithCardResponseDto> CreateCustomerWithCardAsync(
        string email,
        string name,
        string token
    )
    {
        var customer = await mpService.CreateCustomerAsync(email, name);

        var card = await mpService.AddCardAsync(customer.Id, token);

        var cardDto = new CardInCustomerResponseDto(
            card.Id,
            card.LastFourDigits,
            card.ExpirationMonth,
            card.ExpirationYear,
            new PaymentMethodDto(card.PaymentMethod?.Id, card.PaymentMethod?.Name)
        );

        return new CustomerWithCardResponseDto(customer.Id, customer.Email, cardDto);
    }

    public async Task RemoveCardFromWalletAsync(string userId, string cardId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.CustomerId))
            throw new ResourceNotFoundException("Carteira não encontrada.");

        var activeSubscription = await subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
        if (activeSubscription != null && activeSubscription.CardTokenId == cardId)
        {
            throw new InvalidOperationException(
                "Este cartão está vinculado à sua assinatura ativa e não pode ser removido."
            );
        }

        await DeleteCardFromCustomerAsync(user.CustomerId, cardId);
        
        logger.LogInformation("Cartão {CardId} removido da carteira do usuário {UserId}.", cardId, userId);
    }

    public async Task<CardInCustomerResponseDto> AddCardToCustomerAsync(
        string customerId,
        string cardToken
    )
    {
        var mpCard = await mpService.AddCardAsync(customerId, cardToken);

        await cacheService.RemoveAsync($"customer-cards:{customerId}");
        
        var paymentMethodDto = new PaymentMethodDto(
            mpCard.PaymentMethod?.Id,
            mpCard.PaymentMethod?.Name
        );

        return new CardInCustomerResponseDto(
            mpCard.Id,
            mpCard.LastFourDigits,
            mpCard.ExpirationMonth,
            mpCard.ExpirationYear,
            paymentMethodDto
        );
    }

    private async Task DeleteCardFromCustomerAsync(string customerId, string cardId)
    {
        await mpService.DeleteCardAsync(customerId, cardId);
        await cacheService.RemoveAsync($"customer-cards:{customerId}");
    }

    private async Task<List<CardInCustomerResponseDto>> ListCardsFromCustomerAsync(
        string customerId
    )
    {
        var cacheKey = $"customer-cards:{customerId}";
        return await cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await mpService.ListCardsAsync(customerId),
                TimeSpan.FromMinutes(15)
            ) ?? [];
    }
}
