using MeuCrudCsharp.Features.MercadoPago.Clients.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Clients.Interfaces
{
    public interface IClientService
    {
        Task RemoveCardFromWalletAsync(string userId, string cardId);
        Task<WalletCardDto> AddCardToWalletAsync(string userId, string cardToken);

        Task<List<WalletCardDto>> GetUserWalletAsync(string userId);

        Task<CustomerWithCardResponseDto> CreateCustomerWithCardAsync(
            string email,
            string name,
            string token
        );
        Task<CardInCustomerResponseDto> AddCardToCustomerAsync(string customerId, string token);
    }
}
