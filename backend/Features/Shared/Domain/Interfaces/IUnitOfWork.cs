namespace MeuCrudCsharp.Features.Shared.Domain.Interfaces;

public interface IUnitOfWork
{
    Task CommitAsync();

    Task RollbackAsync();
}
