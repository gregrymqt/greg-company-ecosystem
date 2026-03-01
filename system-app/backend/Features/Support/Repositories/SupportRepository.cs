using MeuCrudCsharp.Documents.Models;
using MeuCrudCsharp.Features.Support.Interfaces;
using MongoDB.Driver;

namespace MeuCrudCsharp.Features.Support.Repositories
{
    public class SupportRepository : ISupportRepository
    {
        private readonly IMongoCollection<SupportTicketDocument> _tickets;

        public SupportRepository(IMongoDatabase database)
        {
            _tickets = database.GetCollection<SupportTicketDocument>(
                SupportTicketDocument.CollectionName
            );
        }

        public async Task CreateAsync(SupportTicketDocument ticket)
        {
            await _tickets.InsertOneAsync(ticket);
        }

        public async Task<(
            IEnumerable<SupportTicketDocument> Data,
            long Total
        )> GetAllPaginatedAsync(int page, int pageSize)
        {
            var filter = Builders<SupportTicketDocument>.Filter.Empty;

            var total = await _tickets.CountDocumentsAsync(filter);

            var data = await _tickets
                .Find(filter)
                .SortByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<SupportTicketDocument?> GetByIdAsync(string id)
        {
            return await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateStatusAsync(string id, string newStatus)
        {
            var filter = Builders<SupportTicketDocument>.Filter.Eq(t => t.Id, id);
            var update = Builders<SupportTicketDocument>.Update.Set(t => t.Status, newStatus);

            await _tickets.UpdateOneAsync(filter, update);
        }
    }
}
