# MercadoPago / Payments

Handles all payment processing flows through MercadoPago: PIX, credit card, checkout preferences, and payment history.

## Structure

| Folder | Description |
|---|---|
| `Controllers/` | HTTP endpoints for each payment method |
| `Dtos/` | Request/response records shared across controllers and services |
| `Interfaces/` | Contracts for services and the payment repository |
| `Repositories/` | `PaymentRepository` — EF Core data access, no direct `SaveChanges` |
| `Services/` | Business logic per payment method |

## Controllers

| Controller | Route | Description |
|---|---|---|
| `ConfiguracaoController` | `GET api/configuracao/public-key` | Returns the MercadoPago public key |
| `CreditCardController` | `POST /api/credit/card/process-payment` | Creates a single payment or subscription via credit card (idempotent) |
| `PaymentsController` | `GET api/payments/history` | Returns the authenticated user's payment history |
| `PixController` | `POST api/pix/createpix` | Creates an idempotent PIX payment |
| `PreferenceController` | `POST api/preferences` | Creates a MercadoPago checkout preference |

All controllers extend `MercadoPagoApiControllerBase` and use `HandleException` for unified error responses.

## Services

- **`CreditCardPaymentService`** — manages customer/card creation via `IClientService`, creates or updates `Payments` entity, commits via `IUnitOfWork`, sends real-time updates through `IPaymentNotificationHub`.
- **`PixPaymentService`** — creates a PIX payment via the MercadoPago SDK, persists the result, notifies via Hub.
- **`PreferencePaymentService`** — creates a checkout preference, persists the initial payment record, returns the preference ID.
- **`PaymentService`** — reads payment history from the repository and maps to `PaymentHistoryDto`.
- **`MercadoPagoPaymentService`** — low-level HTTP client to query a payment's status directly from the MercadoPago REST API.

## Idempotency

`CreditCardPaymentService` and `PixPaymentService` cache the first response keyed by `X-Idempotency-Key` (TTL 24 h via `ICacheService`). Duplicate requests return the cached result without re-processing.

## Unit of Work Pattern

Services use `IPaymentRepository` to stage changes and `IUnitOfWork.CommitAsync()` for a single atomic commit. No service calls `SaveChanges` directly.
