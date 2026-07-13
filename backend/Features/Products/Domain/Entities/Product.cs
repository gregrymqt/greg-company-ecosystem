using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;
using MeuCrudCsharp.Features.Products.Domain.Enums;

namespace MeuCrudCsharp.Features.Products.Domain.Entities;

[BsonIgnoreExtraElements]
public class Product : IMongoDocument
{
    public static string CollectionName => "products";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [MongoIndex]
    [BsonElement("tenant_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string TenantId { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("title")]
    [BsonIgnoreIfNull]
    public string? Title { get; set; }

    [BsonElement("description")]
    [BsonIgnoreIfNull]
    public string? Description { get; set; }

    [BsonElement("price")]
    [BsonIgnoreIfNull]
    public decimal? Price { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "BRL";

    [BsonElement("category")]
    public string Category { get; set; } = "Geral";
    
    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public ProductStatus Status { get; set; } = ProductStatus.Raw;
    
    [BsonElement("images")]
    public List<string> Images { get; set; } = new();
    
    [BsonElement("attributes")]
    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    public Dictionary<string, string> Attributes { get; set; } = new();

    [BsonElement("metadata")]
    public ScraperMetadata Metadata { get; set; } = new();
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("last_error")]
    [BsonIgnoreIfNull]
    public string? LastError { get; set; }

    public class ScraperMetadata
    {
        [BsonElement("source_url")]
        public string SourceUrl { get; set; } = string.Empty;

        [BsonElement("last_scraped_at")]
        [BsonIgnoreIfNull]
        public DateTime? LastScrapedAt { get; set; }

        [BsonElement("scraper_version")]
        public string ScraperVersion { get; set; } = string.Empty;
    }
}
