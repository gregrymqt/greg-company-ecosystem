# MercadoPago / Subscriptions

Manages the full lifecycle of recurring subscriptions via MercadoPago's `preapproval` API.

## Responsibilities

- Create recurring subscriptions tied to a plan and user
- Update subscription value and status (admin and user-facing)
- Activate subscriptions from single payments (Pix / Boleto)
- Cache active subscription details per user
- Compensate failed local commits by cancelling the subscription on MercadoPago

## Structure

| Layer | File | Role |
|---|---|---|
| Controllers | `AdminSubscriptionsController` | Admin CRUD for subscriptions |
| Controllers | `UserSubscriptionsController` (`SubscriptionsController`) | End-user subscription details and status change |
| Services | `AdminSubscriptionService` (`SubscriptionService`) | Business logic for admin operations |
| Services | `UserSubscriptionService` | Business logic for user-facing operations |
| Services | `MercadoPagoSubscriptionService` | HTTP calls to MP `preapproval` endpoints |
| Interfaces | `ISubscriptionService` | Contract for admin service |
| Interfaces | `IUserSubscriptionService` | Contract for user service |
| Interfaces | `IMercadoPagoSubscriptionService` | Contract for MP HTTP service |
| Interfaces | `ISubscriptionRepository` | Contract for repository |
| Repositories | `SubscriptionRepository` | EF Core data access (no `SaveChanges` — UnitOfWork pattern) |
| DTOs | `SubscriptionDtos` | Request/response records shared across layers |

## Key Patterns

- **UnitOfWork atomicity**: repositories mark changes in-memory; services call `_unitOfWork.CommitAsync()` as a single commit after all validations pass.
- **Rollback on MP failure**: if MercadoPago returns an error before commit, no local data is persisted (EF change tracker discarded automatically).
- **Compensating transaction**: if the local DB commit fails after a successful MP call, the service cancels the subscription on MP via `CancelSubscriptionAsync`.
- **Cache**: subscription details cached per user via `ICacheService`; invalidated on every status or value update.
- **SubscriptionStatus enum**: string conversions handled by `ToMpString()` / `FromMpString()` extension methods to avoid raw string literals.

## Endpoints

### Admin (`/api/admin/subscriptions`, requires `Admin` role)
| Method | Route | Action |
|---|---|---|
| GET | `/{query}` | Search subscription by external ID or user ID |
| PUT | `/{id}/value` | Update recurring transaction amount |
| PUT | `/{id}/status` | Update subscription status |

### User (`/api/subscriptions`, requires authentication)
| Method | Route | Action |
|---|---|---|
| GET | `/details` | Get active subscription details for current user |
| PUT | `/status` | Change status of current user's subscription |
