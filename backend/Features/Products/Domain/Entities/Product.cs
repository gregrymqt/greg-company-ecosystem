using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Features.Products.Domain.Entities;

[BsonIgnoreExtraElements]
public class Product : IMongoDocument
{
    public static string CollectionName => "products";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [MongoIndex]
    public string TenantId { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "BRL";
    public string Category { get; set; } = "Geral";
    
    // Status: "Raw", "Processing", "Processed", "Failed"
    public string Status { get; set; } = "Raw";
    
    public List<string> Images { get; set; } = new();
    
    public Dictionary<string, string> Attributes { get; set; } = new();

    public ScraperMetadata Metadata { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? LastError { get; set; }

    public class ScraperMetadata
    {
        public DateTime? LastScrapedAt { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public string ScraperVersion { get; set; } = string.Empty;
    }
}
