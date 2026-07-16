using MeuCrudCsharp.Features.Support.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Support.Domain.Interfaces
{
    public interface ISupportRepository
    {
        Task<SupportTicket?> GetByIdAsync(string id);
        Task<IEnumerable<SupportTicket>> GetAllAsync();
        Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize);
        Task CreateAsync(SupportTicket ticket);
        Task UpdateAsync(string id, SupportTicket ticket);
        Task DeleteAsync(string id);
        Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId);
        Task AddResponseAsync(string ticketId, SupportResponse response);
    }
}

