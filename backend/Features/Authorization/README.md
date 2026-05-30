# Feature: Authorization

Implementa a política de autorização baseada em assinatura ativa (`ActiveSubscription`). Qualquer endpoint decorado com essa policy exige que o usuário possua uma assinatura com status `"ativo"` no banco de dados. Admins são isentos da verificação.

## Estrutura

```
Authorization/
├── ActiveSubscriptionRequirement.cs                    # Marcador da política (IAuthorizationRequirement)
├── ActiveSubscriptionHandler.cs                        # Lógica de validação da assinatura
└── SubscriptionAuthorizationMiddlewareResultHandler.cs # Resposta customizada para acesso negado
```

## Como Usar

Decore um endpoint ou controller com a policy:

```csharp
[Authorize(Policy = "ActiveSubscription")]
public async Task<IActionResult> ConteudoExclusivo() { ... }
```

A policy deve estar registrada no `Program.cs` / `AuthExtensions.cs`:

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("ActiveSubscription", policy =>
        policy.Requirements.Add(new ActiveSubscriptionRequirement()));
});

services.AddSingleton<IAuthorizationHandler, ActiveSubscriptionHandler>();
services.AddSingleton<IAuthorizationMiddlewareResultHandler, SubscriptionAuthorizationMiddlewareResultHandler>();
```

## Fluxo de Autorização

```
Requisição chega
    ↓
[JwtBlacklistMiddleware] — token revogado? → 401
    ↓
[AuthorizationMiddleware] — avalia a policy "ActiveSubscription"
    ↓
ActiveSubscriptionHandler.HandleRequirementAsync()
    ├── É Admin?                          → Succeed (acesso liberado)
    ├── userId ausente no token?          → Fail
    └── Tem assinatura status "ativo"?
            ├── Sim                       → Succeed
            └── Não                       → Fail
    ↓
SubscriptionAuthorizationMiddlewareResultHandler
    ├── Falha por outra policy            → comportamento padrão do ASP.NET Core
    └── Falha por ActiveSubscriptionRequirement
            ├── Request com Accept: application/json → 403 JSON
            └── Request de navegador                 → Redirect /Payment/CreditCard
```

## Componentes

### `ActiveSubscriptionRequirement`

Classe marcadora vazia que implementa `IAuthorizationRequirement`. Identifica unicamente a política de assinatura dentro do pipeline de autorização do ASP.NET Core.

### `ActiveSubscriptionHandler`

`AuthorizationHandler` registrado como **singleton**. Usa `IDbContextFactory<ApiDbContext>` para criar um `DbContext` por chamada, evitando problemas de concorrência típicos de singletons com EF Core.

Regras:
- Role `Admin` → acesso automático
- `userId` ausente → falha
- `Subscription` com `UserId == userId && Status == "ativo"` existir → acesso liberado

### `SubscriptionAuthorizationMiddlewareResultHandler`

Personaliza a resposta quando a autorização falha especificamente por `ActiveSubscriptionRequirement`. Distingue chamadas de API de navegação por browser via header `Accept`:

| Tipo de chamada | Resposta |
|-----------------|----------|
| `Accept: application/json` | `403 Forbidden` com JSON `{ error, message, redirectUrl }` |
| Navegação normal | `302 Redirect` para `/Payment/CreditCard` |

Resposta JSON para chamadas de API:

```json
{
  "error": "active_subscription_required",
  "message": "Uma assinatura ativa é necessária para acessar este recurso.",
  "redirectUrl": "/Payment/CreditCard"
}
```
