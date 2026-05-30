# MercadoPago / Webhooks

Receives and processes incoming webhook notifications from MercadoPago, validates their HMAC-SHA256 signature, and dispatches background jobs for each event type.

## Responsibilities

- Validate `x-signature` / `x-request-id` headers against the configured `WebhookSecret`
- Route each notification `type` to the corresponding Hangfire background job
- Return `202 Accepted` to prevent MercadoPago from retrying; return `400` on invalid signatures

## Structure

| Layer | File | Role |
|---|---|---|
| Controller | `WebHookController` | Receives POST from MercadoPago, validates signature, delegates processing |
| Service | `WebhookService` | Signature validation logic + job dispatch switch |
| Interface | `IWebhookService` | Contract for signature validation and processing |
| DTOs | `MercadoPagoNotificationPayloads` | Notification envelope and per-type payload records |

## Notification Types Handled

| `type` value | Background Job |
|---|---|
| `payment` | `ProcessPaymentNotificationJob` |
| `subscription_authorized_payment` | `ProcessRenewalSubscriptionJob` |
| `subscription_preapproval_plan` | `ProcessPlanSubscriptionJob` |
| `subscription_preapproval` | `ProcessCreateSubscriptionJob` |
| `claim` | `ProcessClaimJob` |
| `automatic-payments` | `ProcessCardUpdateJob` |
| `chargeback` / `topic_chargebacks_wh` | `ProcessChargebackJob` |

## Signature Validation

MercadoPago sends `x-signature` and `x-request-id` headers. The service reconstructs the manifest string `id:{dataId};request-id:{xRequestId};ts:{ts};`, computes HMAC-SHA256 with the `WebhookSecret`, and compares to the `v1=` hash in the signature header.

## Endpoint

| Method | Route | Auth |
|---|---|---|
| POST | `/webhook/mercadopago` | None (open — called by MercadoPago servers) |
