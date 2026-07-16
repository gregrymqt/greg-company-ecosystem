using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Files.Domain.Interfaces;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Files.Infrastructure.Persistence.Repositories;

public class FileRepository : IFileRepository
{
    private readonly ApplicationDbContext _context;

    public FileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EntityFile?> GetByIdAsync(Guid id)
        => await _context.EntityFiles.FindAsync(id);

    public async Task AddAsync(EntityFile arquivo)
    {
        await _context.EntityFiles.AddAsync(arquivo);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(EntityFile arquivo)
    {
        _context.EntityFiles.Update(arquivo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(EntityFile arquivo)
    {
        _context.EntityFiles.Remove(arquivo);
        await _context.SaveChangesAsync();
    }
}
