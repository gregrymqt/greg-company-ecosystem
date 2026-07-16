using System.Text.Json;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Products.Domain.Entities;
using MeuCrudCsharp.Features.Products.Infrastructure.Persistence;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Products.Domain.Enums;

namespace MeuCrudCsharp.Features.Products.ImportProduct;

public class ImportProductFromScraperCommandHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public ImportProductFromScraperCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Guid> Handle(ImportProductFromScraperCommand command, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var product = new Product
            {
                TenantId = command.TenantId,
                Status = ProductStatus.Raw,
                Metadata = new Product.ScraperMetadata
                {
                    SourceUrl = command.TargetUrl
                }
            };

            await _productRepository.InsertAsync(product, cancellationToken);

            var outboxEvent = new OutboxEvent
            {
                EventType = "product.import.request",
                Payload = JsonSerializer.Serialize(new
                {
                    ProductId = product.Id,
                    TenantId = command.TenantId,
                    TargetUrl = command.TargetUrl
                })
            };

            await _context.OutboxEvents.AddAsync(outboxEvent, cancellationToken);

            await _unitOfWork.CommitAsync();

            return product.Id;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
