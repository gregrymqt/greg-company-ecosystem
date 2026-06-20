using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MongoDB.Driver;

namespace MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IMongoDbContext _context;
    private IClientSessionHandle? _session;
    private bool _isDisposed;

    public UnitOfWork(IMongoDbContext context)
    {
        _context = context;
    }

    public async Task CommitAsync()
    {
        if (_session == null)
            return;

        try
        {
            await _session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Erro ao persistir alterações no banco de dados. Verifique os logs para mais detalhes.",
                ex
            );
        }
        finally
        {
            _session.Dispose();
            _session = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_session != null)
        {
            await _session.AbortTransactionAsync();
            _session.Dispose();
            _session = null;
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _session?.Dispose();
            _isDisposed = true;
        }
        GC.SuppressFinalize(this);
    }
}