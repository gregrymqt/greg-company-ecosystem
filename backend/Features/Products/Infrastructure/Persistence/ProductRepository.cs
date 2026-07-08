using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Products.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MongoDB.Driver;

namespace MeuCrudCsharp.Features.Products.Infrastructure.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _collection;
    private readonly IUnitOfWork _unitOfWork;

    public ProductRepository(IMongoDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _collection = dbContext.GetCollection<Product>(Product.CollectionName);
        _unitOfWork = unitOfWork;
    }

    public async Task InsertAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (_unitOfWork.Session != null)
        {
            await _collection.InsertOneAsync(_unitOfWork.Session, product, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.InsertOneAsync(product, cancellationToken: cancellationToken);
        }
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Id, product.Id);
        
        if (_unitOfWork.Session != null)
        {
            await _collection.ReplaceOneAsync(_unitOfWork.Session, filter, product, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.ReplaceOneAsync(filter, product, cancellationToken: cancellationToken);
        }
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
        
        if (_unitOfWork.Session != null)
        {
            return await _collection.Find(_unitOfWork.Session, filter).FirstOrDefaultAsync(cancellationToken);
        }
        
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
