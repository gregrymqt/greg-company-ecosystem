using MeuCrudCsharp.Features.Support.Domain.Entities;
using MeuCrudCsharp.Features.Support.Domain.Interfaces;
using MongoDB.Driver;
using MeuCrudCsharp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeuCrudCsharp.Features.Support.Infrastructure.Persistence.Repositories
{
    public class SupportRepository : ISupportRepository
    {
        private readonly IMongoCollection<SupportTicket> _tickets;

        public SupportRepository(IMongoDbContext context)
        {
            _tickets = context.GetCollection<SupportTicket>("support_tickets");
        }

        public async Task<SupportTicket?> GetByIdAsync(string id)
        {
            return await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SupportTicket>> GetAllAsync()
        {
            return await _tickets.Find(_ => true).ToListAsync();
        }

        public async Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize)
        {
            var totalCount = (int)await _tickets.CountDocumentsAsync(Builders<SupportTicket>.Filter.Empty);
            var items = await _tickets.Find(Builders<SupportTicket>.Filter.Empty).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task CreateAsync(SupportTicket ticket)
        {
            await _tickets.InsertOneAsync(ticket);
        }

        public async Task UpdateAsync(string id, SupportTicket ticket)
        {
            await _tickets.ReplaceOneAsync(t => t.Id == id, ticket);
        }

        public async Task DeleteAsync(string id)
        {
            await _tickets.DeleteOneAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId)
        {
            return await _tickets.Find(t => t.UserId == userId).ToListAsync();
        }
    }
}

