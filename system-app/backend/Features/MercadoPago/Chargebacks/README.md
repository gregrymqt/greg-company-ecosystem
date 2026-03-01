# Chargebacks Feature

Manages chargeback records triggered by MercadoPago. Provides paginated listing and detailed view (fetched from the MP API with caching), plus write operations handled by the notification job pipeline.

---

## Folder Structure

```
Features/MercadoPago/Chargebacks/
├── Controllers/
│   └── ChargebackController.cs                        # Admin endpoints (role: Admin)
├── DTOs/
│   └── ChargebackDTO.cs                               # MP API response deserialization models
├── Interfaces/
│   ├── IChargebackRepository.cs                       # Data access contract
│   ├── IChargebackService.cs                          # Business logic contract (read ops)
│   └── IMercadoPagoChargebackIntegrationService.cs    # MP API integration contract
├── Repositories/
│   └── ChargebackRepository.cs                        # EF Core implementation
├── Services/
│   ├── ChargebackService.cs                           # Read operations with cache
│   └── MercadoPagoChargebackIntegrationService.cs     # HTTP calls to MP API
├── ViewModels/
│   └── ChargebackViewModels.cs                        # Response shapes for the frontend
└── README.md
```

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/admin/chargebacks` | Paginated list with optional search and status filter |
| `GET` | `/api/admin/chargebacks/{id}/details` | Full chargeback detail from MP API |

All endpoints require `[Authorize(Roles = "Admin")]`.

### Query Parameters (`GET /chargebacks`)

| Parameter | Type | Description |
|---|---|---|
| `searchTerm` | `string?` | Matches by numeric chargeback ID or customer name |
| `statusFilter` | `string?` | Filters by `ChargebackStatus` enum name |
| `page` | `int` | Page number (default: 1) |

---

## Services

### `ChargebackService`

Handles read operations only. Write operations (create/update from webhooks) live in the notification job pipeline.

- `GetChargebacksAsync` — paginated DB query, cached for 5 minutes per unique filter+page combination
- `GetChargebackDetailAsync` — fetches from MP API via `IMercadoPagoChargebackIntegrationService`, cached for 10 minutes per chargeback ID

### `MercadoPagoChargebackIntegrationService`

Extends `MercadoPagoServiceBase`. Calls `GET v1/chargebacks/{id}` on the MercadoPago API and deserializes the response into `MercadoPagoChargebackResponse`.

---

## DTOs (MP API Response)

| Class | Purpose |
|---|---|
| `MercadoPagoChargebackResponse` | Top-level chargeback data from MP |
| `MercadoPagoChargebackPayment` | Payment reference within a chargeback |
| `MercadoPagoDocumentation` | Uploaded documentation entry |

---

## ViewModels

| Class | Purpose |
|---|---|
| `ChargebacksIndexViewModel` | Paginated list response with filter state |
| `ChargebackSummaryViewModel` | Single row in the list (id, customer, amount, status) |
| `ChargebackDetailViewModel` | Full detail view with documentation files |
| `ChargebackFileViewModel` | Individual uploaded file reference |

---

## Error Handling

All controller actions use `HandleException` from `ApiControllerBase`:
- `AppServiceException` / `InvalidOperationException` → 400
- `ResourceNotFoundException` → 404
- `ExternalApiException` → 400 (re-thrown from integration service)
- Unhandled exceptions → 500
