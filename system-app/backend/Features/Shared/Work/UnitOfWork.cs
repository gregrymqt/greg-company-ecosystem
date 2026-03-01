using MeuCrudCsharp.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Shared.Work;

public class UnitOfWork(ApiDbContext context) : IUnitOfWork
{
    public async Task CommitAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "Erro ao persistir alterações no banco de dados. Verifique os logs para mais detalhes.",
                ex
            );
        }
    }

    public Task RollbackAsync()
    {
        context.ChangeTracker.Clear();
        return Task.CompletedTask;
    }
}