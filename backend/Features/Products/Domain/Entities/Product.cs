using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Features.Products.Domain.Entities;

public class Product : IMongoDocument
{
    public static string CollectionName => "Products";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [MongoIndex]
    public string TenantId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // Status: "Integrating", "Active", "Failed"
    public string Status { get; set; } = "Integrating";
    
    public List<string> Images { get; set; } = new();
    
    public Dictionary<string, string> Attributes { get; set; } = new();

    public ScraperMetadata Metadata { get; set; } = new();

    public class ScraperMetadata
    {
        public DateTime? LastScrapedAt { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public string ScraperVersion { get; set; } = string.Empty;
    }
}
