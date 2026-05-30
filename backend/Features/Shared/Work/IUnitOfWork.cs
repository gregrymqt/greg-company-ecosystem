namespace MeuCrudCsharp.Features.Shared.Work;

public interface IUnitOfWork
{
    Task CommitAsync();

    Task RollbackAsync();
}
