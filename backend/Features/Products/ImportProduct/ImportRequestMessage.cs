using System.Text.Json.Serialization;

namespace MeuCrudCsharp.Features.Products.ImportProduct;

public class ImportRequestMessage
{
    [JsonPropertyName("ProductId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("TenantId")]
    public string TenantId { get; set; } = string.Empty;

    [JsonPropertyName("TargetUrl")]
    public string TargetUrl { get; set; } = string.Empty;
}
