using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Files.Repositories;

public class FileRepository(ApiDbContext context) : IFileRepository
{
    public async Task<EntityFile?> GetByIdAsync(int id)
        => await context.Files.FindAsync(id);

    public async Task AddAsync(EntityFile arquivo)
    {
        await context.Files.AddAsync(arquivo);
    }

    public Task UpdateAsync(EntityFile arquivo)
    {
        context.Files.Update(arquivo);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(EntityFile arquivo)
    {
        context.Files.Remove(arquivo);
        return Task.CompletedTask;
    }
}
