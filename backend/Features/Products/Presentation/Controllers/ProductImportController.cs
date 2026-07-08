using Microsoft.AspNetCore.Mvc;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Products.ImportProduct;

namespace MeuCrudCsharp.Features.Products.Presentation.Controllers;

[Route("api/products")]
public class ProductImportController : ApiControllerBase
{
    private readonly ImportProductFromScraperCommandHandler _commandHandler;

    public ProductImportController(ImportProductFromScraperCommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportProduct([FromBody] ImportProductFromScraperCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var productId = await _commandHandler.Handle(command, cancellationToken);
            return Accepted(new { ProductId = productId, Message = "Import started." });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Failed to import product.");
        }
    }
}
