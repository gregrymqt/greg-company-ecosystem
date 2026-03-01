# Shared

Cross-cutting infrastructure primitives shared across all features.

## Structure

### Token / `AntiforgeryController`

Exposes an unauthenticated endpoint that issues an CSRF antiforgery token pair. The front-end calls this to obtain the request token and header name before submitting state-changing requests.

| Method | Route | Auth |
|---|---|---|
| GET | `/api/antiforgery/token` | None |

Returns `{ token, headerName }` — the client places `token` in the header named `headerName` on subsequent requests.

---

### Work / `IUnitOfWork` + `UnitOfWork`

Implements the Unit of Work pattern over EF Core.

| Method | Behaviour |
|---|---|
| `CommitAsync()` | Calls `SaveChangesAsync()` — persists all tracked changes atomically; wraps `DbUpdateException` in `InvalidOperationException` |
| `RollbackAsync()` | Clears the EF Core `ChangeTracker`, discarding all pending changes without hitting the database |

All repositories in this project mark changes in-memory only; services call `CommitAsync()` as the single commit point to guarantee atomicity.
