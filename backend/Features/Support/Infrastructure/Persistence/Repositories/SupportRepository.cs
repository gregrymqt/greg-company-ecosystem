using MeuCrudCsharp.Features.Support.Domain.Entities;
using MeuCrudCsharp.Features.Support.Domain.Interfaces;
using MeuCrudCsharp.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Support.Infrastructure.Persistence.Repositories
{
    public class SupportRepository : ISupportRepository
    {
        private readonly ApplicationDbContext _context;

        public SupportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupportTicket?> GetByIdAsync(Guid id)
        {
            return await _context.SupportTickets.FindAsync(id);
        }

        public async Task<IEnumerable<SupportTicket>> GetAllAsync()
        {
            return await _context.SupportTickets.ToListAsync();
        }

        public async Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize)
        {
            var totalCount = await _context.SupportTickets.CountAsync();
            var items = await _context.SupportTickets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }

        public async Task CreateAsync(SupportTicket ticket)
        {
            await _context.SupportTickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, SupportTicket ticket)
        {
            _context.SupportTickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket != null)
            {
                _context.SupportTickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SupportTicket>> GetByUserIdAsync(Guid userId)
        {
            return await _context.SupportTickets
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task AddResponseAsync(Guid ticketId, SupportResponse response)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.Responses.Add(response);
                ticket.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
