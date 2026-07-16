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

        Task<IEnumerable<SupportTicketResponseDto>> GetTicketsByUserIdAsync(string userId);
        Task<SupportTicketResponseDto> GetTicketByIdAsync(string id);

        Task UpdateTicketStatusAsync(string id, UpdateSupportTicketDto dto);
        Task AssignTicketAsync(string id, string adminId);
        Task ReplyToTicketAsync(string id, string senderId, string senderRole, ReplyToTicketDto dto);
    }
}
