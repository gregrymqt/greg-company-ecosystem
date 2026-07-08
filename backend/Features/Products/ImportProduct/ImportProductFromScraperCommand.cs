namespace MeuCrudCsharp.Features.Products.ImportProduct;

public class ImportProductFromScraperCommand
{
    public string TenantId { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
}
