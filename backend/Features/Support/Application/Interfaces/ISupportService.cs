using MeuCrudCsharp.Features.Support.Application.DTOs;
using MeuCrudCsharp.Features.Videos.Application.DTOs;

namespace MeuCrudCsharp.Features.Support.Application.Interfaces
{
    public interface ISupportService
    {
        Task CreateTicketAsync(string userId, CreateSupportTicketDto dto);
        Task<PaginatedResultDto<SupportTicketResponseDto>> GetAllTicketsPaginatedAsync(
            int page,
            int pageSize
        );

        Task<SupportTicketResponseDto> GetTicketByIdAsync(string id);

        Task UpdateTicketStatusAsync(string id, UpdateSupportTicketDto dto);
    }
}
