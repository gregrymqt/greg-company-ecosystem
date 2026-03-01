# MercadoPago — Hub Feature

Serviço de notificação em tempo real de status de pagamento via SignalR.

## Responsabilidade

Encapsula o envio de atualizações de pagamento para conexões ativas de um usuário específico, utilizando o `ConnectionMapping` para mapear `userId` → connection IDs do SignalR.

## Estrutura

```
Hub/
├── IPaymentNotificationHub.cs   # Contrato de envio de notificação
└── PaymentNotificationHub.cs    # Implementação via IHubContext<PaymentProcessingHub>
```

## Funcionamento

1. Recebe um `userId` e um `PaymentStatusUpdate`
2. Consulta o `ConnectionMapping<string>` para obter todos os connection IDs ativos daquele usuário
3. Envia o evento `UpdatePaymentStatus` apenas para essas conexões via `IHubContext`

## Integrações

- **SignalR**: `IHubContext<PaymentProcessingHub>` para envio direcionado
- **ConnectionMapping**: Mapeamento de `userId → connectionIds` gerenciado pelo hub de infraestrutura (`Features/Hubs`)
- **PaymentStatusUpdate**: Record definido em `Features/MercadoPago/Notification/Record`
