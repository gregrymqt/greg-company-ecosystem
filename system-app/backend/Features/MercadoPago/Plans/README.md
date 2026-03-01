# MercadoPago / Plans

Manages the full lifecycle of subscription plans: creation, listing, updating, and deactivation. Coordinates between the local database and the MercadoPago `/preapproval_plan` API.

## Structure

| Folder | Description |
|---|---|
| `Controllers/` | Admin and public HTTP endpoints |
| `DTOs/` | Request/response records shared across all layers |
| `Interfaces/` | Contracts for the plan service, repository, and MP API service |
| `Mappers/` | `PlanMapper` — maps `Plan` entities and MP API responses to DTOs |
| `Repositories/` | `PlanRepository` — EF Core data access, no direct `SaveChanges` |
| `Services/` | Business logic (`PlanService`) and MP API client (`MercadoPagoPlanService`) |
| `Utils/` | `PlanUtils` — formatting helpers and update-apply logic |

## Controllers

| Controller | Route | Description |
|---|---|---|
| `AdminPlansController` | `api/admin/plans` | CRUD for plans (create, list, get by id, update, deactivate) |
| `PublicPlansController` | `api/public/plans` | Public, unauthenticated read-only listing of active plans |

Both controllers extend `MercadoPagoApiControllerBase` and use `HandleException` for unified error responses.

## Services

- **`PlanService`** — orchestrates the full plan lifecycle:
  - Creates the local `Plan` entity first (staged), then calls `MercadoPagoPlanService` to create in MP, commits atomically via `IUnitOfWork`.
  - On update, applies changes in memory, updates MP first, then commits — manual field rollback on failure.
  - On delete, marks `IsActive = false`, cancels in MP, then commits — reverts `IsActive` on failure.
  - Invalidates `ICacheService` entries after each write.
  - `GetActiveDbPlansAsync` serves public/fast reads from the database.
  - `GetActiveApiPlansAsync` cross-references the MP API with local plan records.

- **`MercadoPagoPlanService`** — thin HTTP wrapper over the MercadoPago `/preapproval_plan` endpoints (create, update, cancel, get, search). Extends `MercadoPagoServiceBase`.

## DTOs

| Record | Description |
|---|---|
| `CreatePlanDto` | Input for plan creation |
| `UpdatePlanDto` | Partial update input (null fields are ignored) |
| `PlanDto` | Public-facing display DTO (name, price, features, billing info) |
| `PlanEditDto` | Edit-modal DTO with raw financial values |
| `PlanResponseDto` | MP API plan response |
| `AutoRecurringDto` | Recurring billing details (frequency, amount, currency) |
| `PagedResultDto<T>` | Generic paginated wrapper with metadata |

## Unit of Work Pattern

`PlanRepository` stages all changes; `PlanService` calls `IUnitOfWork.CommitAsync()` at the correct point. Unhandled exceptions before commit result in automatic EF Core rollback. For tracked entities (update/delete), fields are manually reverted when the external MP call fails before commit.
