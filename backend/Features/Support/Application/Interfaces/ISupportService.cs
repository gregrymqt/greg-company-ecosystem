using MeuCrudCsharp.Features.Support.Application.DTOs;
using MeuCrudCsharp.Features.Shared.Application.DTOs;

namespace MeuCrudCsharp.Features.Support.Application.Interfaces
{
    public interface ISupportService
    {
        Task CreateTicketAsync(Guid userId, CreateSupportTicketDto dto);
        Task<PaginatedResultDto<SupportTicketResponseDto>> GetAllTicketsPaginatedAsync(
            int page,
            int pageSize
        );

        Task<IEnumerable<SupportTicketResponseDto>> GetTicketsByUserIdAsync(Guid userId);
        Task<SupportTicketResponseDto> GetTicketByIdAsync(Guid id);

        Task UpdateTicketStatusAsync(Guid id, UpdateSupportTicketDto dto);
        Task AssignTicketAsync(Guid id, string adminId);
        Task ReplyToTicketAsync(Guid id, Guid senderId, string senderRole, ReplyToTicketDto dto);
    }
}
