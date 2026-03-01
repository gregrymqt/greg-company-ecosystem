# Feature: Base Controllers

Esta feature fornece classes base abstratas para os controllers da API, garantindo consistência em autenticação, tratamento de exceções e outras políticas transversais.

## Estrutura

```
Base/
└── ApiControllerBase.cs   # Contém as classes base para todos os controllers
```

## Componentes Principais

### `ApiControllerBase`

A classe fundamental para todos os controllers da API no ecossistema.

**Responsabilidades:**
- **Autenticação Padrão:** Decorada com `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`, exige que todas as rotas sejam protegidas por um token JWT válido por padrão.
- **Tratamento de Exceções Centralizado:** Implementa o método `HandleException` para padronizar as respostas de erro da API.

#### Fluxo de Tratamento de Erros (`HandleException`)

O método `HandleException` captura exceções e as converte em respostas HTTP padronizadas:

| Exceção | Status Code | Corpo da Resposta (JSON) |
|---|---|---|
| `AppServiceException`, `InvalidOperationException` | `400 Bad Request` | `{ success, message, error }` |
| `ResourceNotFoundException` | `404 Not Found` | `{ success, message }` |
| Qualquer outra `Exception` | `500 Internal Server Error` | `{ success, message, details }` |

### `MercadoPagoApiControllerBase`

Uma especialização de `ApiControllerBase` destinada a controllers que interagem com a API do MercadoPago.

**Funcionalidades Adicionais:**
- **Rate Limiting:** Decorada com `[RateLimit(5, 60)]`, limita as requisições para 5 chamadas a cada 60 segundos, protegendo contra abuso e respeitando os limites da API externa.

## Como Usar

### Herança Padrão

Para um controller padrão, herde de `ApiControllerBase` e use `HandleException` em blocos `try-catch`.

```csharp
[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }
        catch (Exception ex)
        {
            // Centraliza a lógica de erro, retornando 404, 400 ou 500 conforme a exceção.
            return HandleException(ex, "Falha ao buscar o produto.");
        }
    }
}
```

### Herança para Endpoints do MercadoPago

Para controllers que lidam com o MercadoPago, herde de `MercadoPagoApiControllerBase` para obter o rate limiting automaticamente.

```csharp
[Route("api/mp/payments")]
public class MercadoPagoPaymentsController : MercadoPagoApiControllerBase
{
    // ... a lógica do controller já estará protegida por rate limiting.
}
```

