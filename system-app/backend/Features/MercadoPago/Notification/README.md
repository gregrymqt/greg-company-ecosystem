# MercadoPago — Notification Feature

Serviços de processamento de notificações assíncronas do Mercado Pago, acionados pelos Jobs do Hangfire.

## Responsabilidade

Cada serviço recebe os dados brutos de um webhook, aplica lógica de negócio (idempotência, atualização de entidades, commit via UoW) e envia e-mails de confirmação após a persistência.

## Estrutura

```
Notification/
├── Interfaces/
│   ├── ICardUpdateNotificationService.cs
│   ├── IChargeBackNotificationService.cs
│   ├── IClaimNotificationService.cs
│   ├── INotificationPayment.cs
│   ├── IPlanUpdateNotificationService.cs
│   ├── ISubscriptionCreateNotificationService.cs
│   └── ISubscriptionNotificationService.cs
├── Record/
│   ├── PaymentMetadata.cs          # UserId + PlanPublicId extraídos do ExternalReference
│   └── PaymentStatusUpdate.cs      # Payload de atualização de status para SignalR
└── Services/
    ├── CardUpdateNotificationService.cs
    ├── ChargeBackNotificationService.cs
    ├── ClaimNotificationService.cs
    ├── NotificationPaymentService.cs
    ├── PlanUpdateNotificationService.cs
    ├── SubscriptionCreateNotificationService.cs
    └── SubscriptionRenewalNotificationService.cs
```

## Serviços e Fluxos

| Serviço | Evento | Fluxo resumido |
|---------|--------|----------------|
| `CardUpdateNotificationService` | Atualização de cartão | Busca assinatura ativa → busca cartão na API MP → atualiza `CardTokenId` + `LastFourCardDigits` → commit → e-mail |
| `ChargeBackNotificationService` | Chargeback | Busca detalhes na API MP → atualiza Payment para `chargeback` → cancela Subscription → cria/atualiza Chargeback → commit → e-mail |
| `ClaimNotificationService` | Claim pós-compra | Busca detalhes na API MP → localiza usuário via Payment/Subscription → cria ou atualiza Claim → commit → e-mail (só para novas) |
| `NotificationPaymentService` | Pagamento | Busca pagamento local → verifica idempotência → busca status no MP → `approved/rejected/refunded` → cria assinatura se necessário → commit → e-mail |
| `PlanUpdateNotificationService` | Atualização de plano | Busca plano na API MP → compara com banco → atualiza campos divergentes → commit → e-mail ao admin |
| `SubscriptionCreateNotificationService` | Criação de assinatura | Busca assinatura por ExternalId → verifica idempotência → seta `active` → commit → e-mail |
| `SubscriptionRenewalNotificationService` | Renovação de assinatura | Busca assinatura por PaymentId → verifica idempotência → calcula nova data → atualiza → commit → e-mail |

## Padrão Comum

1. Valida payload / pré-condições
2. Executa operações em repositories (sem `SaveChanges` direto)
3. Chama `unitOfWork.CommitAsync()` — único ponto de persistência
4. Envia e-mail somente após commit bem-sucedido
5. Em exceção: loga + `throw` (rollback automático via UoW)
