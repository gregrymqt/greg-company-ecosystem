using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Files.Domain.Interfaces;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.Files.Infrastructure.Persistence.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IMongoCollection<EntityFile> _files;

    public FileRepository(IMongoDbContext context)
    {
        _files = context.GetCollection<EntityFile>("files");
    }

    public async Task<EntityFile?> GetByIdAsync(string id)
        => await _files.Find(f => f.Id == id).FirstOrDefaultAsync();

    public async Task AddAsync(EntityFile arquivo)
    {
        await _files.InsertOneAsync(arquivo);
    }

    public async Task UpdateAsync(EntityFile arquivo)
    {
        await _files.ReplaceOneAsync(f => f.Id == arquivo.Id, arquivo);
    }

    public async Task DeleteAsync(EntityFile arquivo)
    {
        await _files.DeleteOneAsync(f => f.Id == arquivo.Id);
    }
}

