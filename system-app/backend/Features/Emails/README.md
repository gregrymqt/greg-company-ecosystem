# Emails Feature

Handles transactional email sending via SendGrid, using Razor view templates to generate HTML bodies.

---

## Folder Structure

```
Features/Emails/
├── Interfaces/
│   ├── IEmailSenderService.cs          # Contract for sending emails
│   └── IRazorViewToStringRenderer.cs   # Contract for rendering Razor views to string
├── Services/
│   ├── SendGridEmailSenderService.cs   # SendGrid implementation of IEmailSenderService
│   └── RazorViewToStringRenderer.cs    # Renders .cshtml templates to HTML strings
├── ViewModels/
│   ├── CardUpdateEmailViewModel.cs
│   ├── ChargebackReceivedEmailViewModel.cs
│   ├── ClaimReceivedEmailViewModel.cs
│   ├── ConfirmationEmailViewModel.cs
│   ├── PlanUpdateAdminNotificationViewModel.cs
│   ├── RefundConfirmationEmailViewModel.cs
│   ├── RejectionEmailViewModel.cs
│   ├── RenewalEmailViewModel.cs
│   └── SubscriptionCreateEmailViewModel.cs
└── README.md
```

---

## Services

### `SendGridEmailSenderService`

Implements `IEmailSenderService`. Sends emails through the SendGrid HTTP API.

- Validates `to`, `subject`, and `htmlBody` before sending.
- Throws `AppServiceException` on validation failure or SendGrid API errors.
- Re-throws `AppServiceException` without wrapping when it originates internally.

**Configuration** — binds from `appsettings.json` via `SendGridOptions`:

```json
{
  "SendGrid": {
    "ApiKey": "...",
    "FromEmail": "noreply@example.com",
    "FromName": "Greg Company"
  }
}
```

### `RazorViewToStringRenderer`

Implements `IRazorViewToStringRenderer`. Renders `.cshtml` Razor views to HTML strings outside of a standard HTTP request cycle, enabling template-based email bodies.

**Usage:**

```csharp
var html = await _renderer.RenderViewToStringAsync("/Views/Emails/Confirmation.cshtml", viewModel);
await _emailSender.SendEmailAsync(to, subject, html, plainText);
```

---

## ViewModels

| ViewModel | Purpose | Key Fields |
|---|---|---|
| `ConfirmationEmailViewModel` | Purchase confirmation | `CustomerName`, `AccessContentUrl`, `SiteUrl` |
| `SubscriptionCreateEmailViewModel` | New subscription | `CustomerName`, `PlanName` |
| `RenewalEmailViewModel` | Subscription renewed | `CustomerName`, `PlanName`, `NextRenewalDate` |
| `RejectionEmailViewModel` | Payment rejected | `CustomerName`, `PaymentPageUrl`, `SiteUrl` |
| `RefundConfirmationEmailViewModel` | Refund processed | `CustomerName`, `AccountUrl`, `SiteUrl` |
| `CardUpdateEmailViewModel` | Card updated | `CustomerName` |
| `PlanUpdateAdminNotificationViewModel` | Admin plan change alert | `CustomerName`, `OldPlan`, `NewPlan` |
| `ChargebackReceivedEmailViewModel` | Chargeback notification | `CustomerName`, `TransactionId` |
| `ClaimReceivedEmailViewModel` | Claim notification | `CustomerName`, `ClaimId` |

---

## Error Handling

- Missing or empty recipient/subject/body → `AppServiceException` (400)
- SendGrid API failure → `AppServiceException` (400) with logged details
- Internal `AppServiceException` is re-thrown without wrapping
