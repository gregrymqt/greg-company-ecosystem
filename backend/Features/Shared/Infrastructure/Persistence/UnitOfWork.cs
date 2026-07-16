using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _isDisposed = false;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IDbContextTransaction? Transaction => _transaction;

    public async Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitAsync()
    {
        if (_transaction == null) return;

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _transaction?.Dispose();
            _isDisposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
