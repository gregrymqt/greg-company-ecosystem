using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using RabbitMQ.Client;
using MeuCrudCsharp.Features.Base.Workers;
using MeuCrudCsharp.Features.Products.Infrastructure.Persistence;
using MeuCrudCsharp.Features.Shared.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MeuCrudCsharp.Features.Products.Shared.Infrastructure.Workers;

public class ProductImportCompletedConsumer : RabbitMqConsumerBase
{
    private readonly IServiceProvider _serviceProvider;
    
    protected override string QueueName => "product.import.completed";

    public ProductImportCompletedConsumer(
        IServiceProvider serviceProvider,
        ILogger<ProductImportCompletedConsumer> logger,
        IConnection connection) : base(connection, logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<ImportCompletedPayload>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (payload == null || string.IsNullOrEmpty(payload.ProductId))
        {
            throw new Exception("Invalid payload or missing ProductId.");
        }

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var product = await repository.GetByIdAsync(payload.ProductId, cancellationToken);
        
        if (product == null)
        {
            throw new Exception($"Product with ID {payload.ProductId} not found.");
        }

        if (payload.Success)
        {
            product.Title = payload.Title ?? product.Title;
            product.Description = payload.Description ?? product.Description;
            product.Price = payload.Price > 0 ? payload.Price : product.Price;
            
            if (payload.Images != null && payload.Images.Count > 0)
            {
                product.Images = payload.Images;
            }
            
            if (payload.Attributes != null && payload.Attributes.Count > 0)
            {
                foreach (var attr in payload.Attributes)
                {
                    product.Attributes[attr.Key] = attr.Value;
                }
            }

            product.Status = "Active";
            _logger.LogInformation("Product {ProductId} imported successfully and activated.", product.Id);
        }
        else
        {
            product.Status = "Failed";
            product.Metadata ??= new Domain.Entities.Product.ScraperMetadata();
            product.Attributes["LastError"] = payload.Error ?? "Unknown error during import.";
            
            _logger.LogWarning("Product {ProductId} failed to import. Error: {Error}", product.Id, payload.Error);
        }

        await repository.UpdateAsync(product, cancellationToken);

        var cacheKey = $"products_{product.TenantId}";
        await cache.RemoveAsync(cacheKey, cancellationToken);

        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();
        await hubContext.Clients.Group(product.TenantId).SendAsync("ProductImportStatusUpdated", new 
        { 
            productId = product.Id, 
            tenantId = product.TenantId, 
            status = product.Status 
        }, cancellationToken);
    }

    private class ImportCompletedPayload
    {
        public bool Success { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<string>? Images { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public string? Error { get; set; }
    }
}
