using MeuCrudCsharp.Features.Support.Domain.Entities;

namespace MeuCrudCsharp.Features.Support.Domain.Interfaces
{
    public interface ISupportRepository
    {
        Task<SupportTicket?> GetByIdAsync(Guid id);
        Task<IEnumerable<SupportTicket>> GetAllAsync();
        Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize);
        Task CreateAsync(SupportTicket ticket);
        Task UpdateAsync(Guid id, SupportTicket ticket);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<SupportTicket>> GetByUserIdAsync(Guid userId);
        Task AddResponseAsync(Guid ticketId, SupportResponse response);
    }
}
