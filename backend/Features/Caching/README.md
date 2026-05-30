# Feature: Caching

Implementa uma camada de cache distribuído utilizando **Redis** para otimizar a performance da aplicação, reduzir a carga no banco de dados e acelerar a entrega de dados. A feature abstrai a complexidade do `IDistributedCache` do ASP.NET Core através de um serviço dedicado, `ICacheService`.

## Estrutura

```
Caching/
├── Interfaces/
│   └── ICacheService.cs       # Contrato principal para operações de cache
├── Record/
│   └── CachedResponse.cs      # DTO para cache de respostas HTTP completas
└── Services/
    └── CacheService.cs        # Implementação com Redis, JSON e logging
```

## Componentes Principais

### `ICacheService`
A interface `ICacheService` é o contrato público que desacopla as features da implementação de cache. Ela define os métodos essenciais para interagir com o cache de forma agnóstica.

### `CacheService`
A implementação concreta que utiliza `IDistributedCache` (configurado para Redis). Suas responsabilidades incluem:
- **Serialização/Desserialização:** Converte objetos C# para JSON (usando `System.Text.Json`) antes de armazenar e o contrário ao ler.
- **Tratamento de Erros:** Captura exceções, como `RedisConnectionException`, e registra logs detalhados sem quebrar a aplicação. Em caso de falha, atua como um "cache miss".
- **Gerenciamento de Expiração:** Aplica um tempo de expiração padrão (`10 minutos`) ou um tempo customizado.

### `CachedResponse`
Um `record` simples usado para armazenar o corpo e o `StatusCode` de uma resposta HTTP. É útil para estratégias de cache de output (Output Caching), onde a resposta inteira de um endpoint é armazenada.

## Como Usar

O serviço deve ser injetado em outras classes de serviço (Services) ou repositórios onde o cache é necessário.

### 1. Injeção de Dependência

Primeiro, injete `ICacheService` no construtor da sua classe.

```csharp
public class MyAwesomeService(ICacheService cacheService, IMyRepository repository)
{
    // ...
}
```

### 2. Padrão Cache-Aside (`GetOrCreateAsync`)

Este é o método mais comum. Ele tenta buscar um item do cache. Se não encontrar (miss), ele executa a função `factory` (que geralmente busca os dados do banco), salva o resultado no cache e o retorna.

```csharp
public async Task<ProductDto> GetProductByIdAsync(int productId)
{
    string cacheKey = $"product:{productId}";

    var product = await _cacheService.GetOrCreateAsync(
        cacheKey,
        async () => await _repository.GetByIdAsync(productId), // Factory executada apenas em cache miss
        TimeSpan.FromHours(1) // Expiração customizada (opcional)
    );

    return product;
}
```

### 3. Invalidação de Cache (`RemoveAsync`)

Após uma operação de escrita (Create, Update, Delete), o cache correspondente deve ser invalidado para evitar dados obsoletos.

```csharp
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);
    
    string cacheKey = $"product:{product.Id}";
    await _cacheService.RemoveAsync(cacheKey); // Remove a chave do cache
}
```

### 4. Invalidação em Massa por Versão (`InvalidateCacheByKeyAsync`)

Para invalidar um grupo de chaves relacionadas (ex: uma lista de produtos), pode-se usar uma chave de versão. Ao atualizar a versão, todas as chaves que dependem dela se tornam obsoletas.

```csharp
// Para buscar a lista
var version = await _cacheService.GetOrCreateAsync("products_version", () => Task.FromResult(Guid.NewGuid().ToString()));
string cacheKey = $"products_all:{version}";
var products = await _cacheService.GetOrCreateAsync(cacheKey, () => _repository.GetAllAsync());

// Para invalidar
await _cacheService.InvalidateCacheByKeyAsync("products_version"); // Isso gera um novo GUID para a versão
```

## Referência da API (`ICacheService`)

| Método | Retorno | Descrição |
|--------|---------|-----------|
| `GetOrCreateAsync<T>(key, factory, expire)` | `Task<T?>` | **Padrão Cache-Aside.** Tenta obter um item; se falhar, executa a `factory`, armazena e retorna o resultado. |
| `GetAsync<T>(key)` | `Task<T?>` | Obtém um item diretamente do cache. Retorna `default` se não for encontrado. |
| `RemoveAsync(key)` | `Task` | Remove um item específico do cache. Usado para invalidação manual. |
| `InvalidateCacheByKeyAsync(versionKey)` | `Task` | **Invalidação por versão.** Atualiza uma chave de versão com um novo GUID, invalidando efetivamente todas as chaves que a utilizam. |
| `IncrementAsync(key, expiration)` | `Task<long>` | Incrementa atomicamente um valor numérico. Ideal para contadores ou rate limiting. |

## Configuração

A configuração do serviço é feita no `Program.cs` (ou em um método de extensão de `IServiceCollection`).

```csharp
// Em Program.cs

// 1. Adicionar e configurar o cliente Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "GregCompany_"; // Prefixo para evitar colisões de chave
});

// 2. Registrar o serviço de cache como Singleton
builder.Services.AddSingleton<ICacheService, CacheService>();
```

## Tratamento de Falhas

O `CacheService` foi projetado para ser resiliente. Se uma conexão com o Redis falhar durante uma operação, o serviço:
1.  Registra um erro detalhado usando `ILogger`.
2.  Retorna um valor padrão (`default` ou `null`).

Isso garante que uma indisponibilidade do Redis não cause uma falha catastrófica na aplicação. O sistema continuará a funcionar, tratando a falha como um "cache miss" e buscando os dados da fonte primária (banco de dados).

---


