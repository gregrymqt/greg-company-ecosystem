namespace MeuCrudCsharp.Features.Shared.Domain.Interfaces;

using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork
{
    IDbContextTransaction? Transaction { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
