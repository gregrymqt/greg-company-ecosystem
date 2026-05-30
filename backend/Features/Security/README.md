# Security

Cross-cutting security primitives applied as action filter attributes.

## Files

### `RateLimitAttribute`

An `ActionFilterAttribute` that enforces per-user rate limiting on any controller action or class it decorates.

**Usage:**
```csharp
[RateLimit(5, 60)] // max 5 requests per 60 seconds per user
public async Task<IActionResult> SomeAction() { ... }
```

**How it works:**
1. Reads the current user's ID from JWT claims (`"id"`).
2. Resolves `ICacheService` from the request service container.
3. Builds a unique cache key: `rate_limit:{userId}:{actionDisplayName}`.
4. Increments an atomic counter via `ICacheService.IncrementAsync` with the given TTL.
5. Returns `429 Too Many Requests` if the counter exceeds the configured limit; otherwise passes through to the action.

**Notes:**
- Unauthenticated requests (no user ID) bypass the limiter and proceed normally.
- If `ICacheService` is not registered, the request is allowed through.
- The counter TTL resets with each new window (`_seconds`).
