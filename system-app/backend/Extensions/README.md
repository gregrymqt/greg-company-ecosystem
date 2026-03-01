# Extensions

Static extension methods that configure the ASP.NET Core host at startup. Each file owns a distinct concern; `Program.cs` composes them in order.

## Files

| File | Extension method(s) | Responsibility |
|---|---|---|
| `AuthExtensions.cs` | `AddAuth`, `UseAuthFeatures` | JWT Bearer + Google OAuth + `ActiveSubscription` / `RequireJwtToken` policies; `JwtBlacklistMiddleware` pipeline |
| `DependencyInjectionExtensions.cs` | `AddApplicationServices` | Scrutor assembly scan for all feature Services/Repositories; manual registration of Hangfire jobs, `IOptions<>` configs, `ConnectionMapping<string>`, FFmpeg settings |
| `PersistenceExtensions.cs` | `AddPersistence` | EF Core DbContextFactory, ASP.NET Core Identity (`Users`/`Roles`), MongoDB singleton, conditional Redis or in-memory cache/Hangfire |
| `PipelineExtensions.cs` | `UseAppPipeline` | Ordered middleware pipeline: error pages → migrations → MongoDB indexes → routing/CORS → network monitoring → auth → Hangfire dashboard → SignalR hubs → controllers |
| `WebAppBuilderExtensions.cs` | `AddCoreServices` | Controllers (camelCase JSON), Razor Pages with JWT authorization conventions, Swagger, SignalR |
| `WebServicesExtensions.cs` | `AddWebServices`, `UseGregNetworkMonitoring`, `ConfigureCookiePolicies` | MercadoPago `HttpClient` with Polly retry + circuit-breaker; CORS policy; `ForwardedHeaders`; cookie policies; MCP network log middleware |

## Startup composition order (`Program.cs`)

```
builder.AddCoreServices()
builder.AddApplicationServices()
builder.AddPersistence()
builder.AddAuth()
builder.AddWebServices()

await app.UseAppPipeline()   // includes UseAuthFeatures() internally
```

## Key patterns

- **Scrutor scan**: `AddApplicationServices` auto-registers every class under `Features/**/{Services,Repositories,Notification}` namespaces as its implemented interface with scoped lifetime. New features are discovered automatically when their namespace matches the registered pattern.
- **Conditional persistence**: `USE_REDIS` env var switches between `StackExchangeRedisCache` + `Hangfire.Redis` and `DistributedMemoryCache` + `Hangfire.InMemory`.
- **MongoDB index bootstrap**: `ConfigureMongoDbIndexes` scans all `IMongoDocument` implementations at startup and creates indexes declared via `[MongoIndex]` attributes.
- **Polly resilience**: MercadoPago `HttpClient` has a 30 s timeout, 3-attempt exponential-backoff retry, and a 5-fault circuit breaker (30 s break duration).
- **MCP network monitor**: `UseGregNetworkMonitoring` posts method/path/status to `http://localhost:8888/log` after every response; failure is silent so the app is unaffected when the MCP server is offline.
