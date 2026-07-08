using MeuCrudCsharp.Features.Products.Domain.Entities;

namespace MeuCrudCsharp.Features.Products.Infrastructure.Persistence;

public interface IProductRepository
{
    Task InsertAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
