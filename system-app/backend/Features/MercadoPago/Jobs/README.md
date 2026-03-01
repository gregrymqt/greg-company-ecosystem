# MercadoPago — Jobs Feature

Infraestrutura de processamento assíncrono de eventos do Mercado Pago via Hangfire.

## Responsabilidade

Recebe payloads de webhooks, valida-os minimamente e delega toda a lógica de negócio para os respectivos serviços de notificação. Cada job tem retry automático (3 tentativas, intervalo de 60s).

## Estrutura

```
Jobs/
├── Interfaces/
│   ├── IJob.cs                            # Contrato genérico: ExecuteAsync(TResource)
│   └── IQueueService.cs                   # Contrato de enfileiramento: EnqueueJobAsync<TJob, TResource>
├── Job/
│   ├── ProcessCardUpdateJob.cs            # Atualização de cartão de crédito
│   ├── ProcessChargebackJob.cs            # Chargeback (contestação de pagamento)
│   ├── ProcessClaimJob.cs                 # Claim (reclamação pós-compra)
│   ├── ProcessCreateSubscriptionJob.cs    # Criação de assinatura
│   ├── ProcessPaymentNotificationJob.cs   # Notificação de pagamento
│   ├── ProcessPlanSubscriptionJob.cs      # Atualização de plano
│   └── ProcessRenewalSubscriptionJob.cs   # Renovação de assinatura
└── Services/
    └── BackgroundJobQueueService.cs       # Implementação via Hangfire IBackgroundJobClient
```

## Padrão de Cada Job

1. Valida payload (nulo / ID vazio) → descarta sem relançar (evita retentativas inúteis)
2. Valida formato do ID quando aplicável (`long.TryParse`)
3. Delega para o serviço especializado (`VerifyAndProcess*`)
4. Em caso de exceção: loga + `throw` para Hangfire aplicar a política de retentativas

## Jobs e Seus Serviços

| Job | Serviço delegado |
|-----|-----------------|
| `ProcessCardUpdateJob` | `ICardUpdateNotificationService` |
| `ProcessChargebackJob` | `IChargeBackNotificationService` |
| `ProcessClaimJob` | `IClaimNotificationService` |
| `ProcessCreateSubscriptionJob` | `ISubscriptionCreateNotificationService` |
| `ProcessPaymentNotificationJob` | `INotificationPayment` |
| `ProcessPlanSubscriptionJob` | `IPlanUpdateNotificationService` |
| `ProcessRenewalSubscriptionJob` | `ISubscriptionNotificationService` |

## Enfileiramento

```csharp
await queueService.EnqueueJobAsync<ProcessPaymentNotificationJob, PaymentNotificationData>(payload);
```

O `BackgroundJobQueueService` serializa a chamada via `IBackgroundJobClient.Enqueue<TJob>` do Hangfire.
