using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Data;
using MongoDB.Driver;

namespace MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IMongoDbContext _context;
    private IClientSessionHandle? _session;
    private bool _isDisposed = false;

    public UnitOfWork(IMongoDbContext context)
    {
        _context = context;
    }

    // Expõe a sessão atual para que os Repositories a utilizem nos métodos do Mongo
    public IClientSessionHandle? Session => _session;

    public async Task BeginTransactionAsync()
    {
        if (_session == null)
        {
            _session = await _context.StartSessionAsync();
            _session.StartTransaction();
        }
    }

    public async Task CommitAsync()
    {
        if (_session == null) return;

        try
        {
            await _session.CommitTransactionAsync();
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