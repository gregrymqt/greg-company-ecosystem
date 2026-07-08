namespace MeuCrudCsharp.Features.Shared.Domain.Interfaces;

using MongoDB.Driver;

public interface IUnitOfWork
{
    IClientSessionHandle? Session { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
