# MercadoPago / Refunds

Handles refund requests for approved payments, coordinating between the local database and the MercadoPago refund API.

## Structure

| Folder | Description |
|---|---|
| `Controllers/` | HTTP endpoint to request a refund |
| `DTOs/` | Request/response records |
| `Interfaces/` | Contracts for the refund service and notification |
| `Notifications/` | SignalR notification implementation |
| `Services/` | Core refund business logic |

## Controller

`RefundsController` — `POST api/refunds/{paymentId}`

Validates that `paymentId` is numeric, delegates to `IRefundService`, and uses `HandleException` for unified error responses.

## Service

`RefundService` orchestrates the full refund flow:
1. Fetches the payment (with linked subscription) from `IPaymentRepository`.
2. Validates: payment must exist, status must be `approved`/`aprovada`, and be within the 7-day window.
3. Calls the MercadoPago `/v1/payments/{id}/refunds` endpoint.
4. Updates `payment.Status = "refunded"` and, if a subscription is linked, `subscription.Status = "refund_pending"`.
5. Commits all changes atomically via `IUnitOfWork`. On failure, re-throws so EF Core rolls back automatically.

## Notifications

`RefundNotification` implements `IRefundNotification` using SignalR. It looks up the user's active connection IDs via `ConnectionMapping<string>` and sends a `ReceiveRefundStatus` event with the current status and message.
