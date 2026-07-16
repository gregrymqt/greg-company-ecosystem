using System.ComponentModel.DataAnnotations;
using MeuCrudCsharp.Features.Products.Domain.Enums;

namespace MeuCrudCsharp.Features.Products.Domain.Entities;

public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TenantId { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string Currency { get; set; } = "BRL";

    public string Category { get; set; } = "Geral";

    public ProductStatus Status { get; set; } = ProductStatus.Raw;

    public List<string> Images { get; set; } = new();

    public Dictionary<string, string> Attributes { get; set; } = new();

    public ScraperMetadata Metadata { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string? LastError { get; set; }

    public class ScraperMetadata
    {
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime? LastScrapedAt { get; set; }
        public string ScraperVersion { get; set; } = string.Empty;
    }
}
