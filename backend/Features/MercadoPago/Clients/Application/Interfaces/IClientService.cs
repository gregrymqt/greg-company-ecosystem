using MeuCrudCsharp.Features.MercadoPago.Clients.Application.DTOs;

namespace MeuCrudCsharp.Features.MercadoPago.Clients.Application.Interfaces
{
    public interface IClientService
    {
        Task RemoveCardFromWalletAsync(Guid userId, string cardId);
        Task<WalletCardDto> AddCardToWalletAsync(Guid userId, string cardToken);

        Task<List<WalletCardDto>> GetUserWalletAsync(Guid userId);

        Task<CustomerWithCardResponseDto> CreateCustomerWithCardAsync(
            string email,
            string name,
            string token
        );
        Task<CardInCustomerResponseDto> AddCardToCustomerAsync(string customerId, string token);
    }
}
