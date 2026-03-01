# MercadoPago — Clients Feature

Gerenciamento da carteira de cartões dos usuários integrado ao sistema de Customers do Mercado Pago.

## Responsabilidade

Sincroniza usuários locais com Customers no Mercado Pago e permite que cada usuário gerencie seus cartões salvos (wallet), com proteção contra remoção de cartão vinculado a assinatura ativa.

## Estrutura

```
Clients/
├── Controllers/
│   └── UserWalletController.cs        # Endpoints da carteira do usuário logado
├── DTOs/
│   └── ClientDtos.cs                  # Records de integração com MP e classes de resposta ao frontend
├── Interfaces/
│   ├── IClientMercadoPagoService.cs   # Contrato para operações diretas na API do MP
│   └── IClientService.cs              # Contrato para lógica de negócio da carteira
├── Services/
│   ├── ClientMercadoPagoService.cs    # Implementação HTTP/SDK do Mercado Pago
│   └── ClientService.cs              # Orquestra banco local + MP + cache
```

## Endpoints

### Wallet (`/api/v1/wallet`)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/` | Lista os cartões salvos do usuário logado |
| POST | `/` | Adiciona um novo cartão via token |
| DELETE | `/{cardId}` | Remove um cartão (bloqueado se vinculado à assinatura ativa) |

## Fluxos Principais

### Adicionar cartão
1. Verifica se o usuário já tem `CustomerId` no banco local
2. Se não tiver, cria um Customer no Mercado Pago e salva o ID via `UnitOfWork`
3. Adiciona o cartão ao Customer e invalida o cache

### Listar cartões
- Busca cartões do MP com cache de 15 minutos (`customer-cards:{customerId}`)
- Marca o cartão que está vinculado à assinatura ativa (`IsSubscriptionActiveCard`)

### Remover cartão
- Bloqueia remoção se o cartão for o `CardTokenId` da assinatura ativa (`InvalidOperationException`)
- Remove na API do MP e invalida o cache

## Integrações

- **Mercado Pago SDK/API**: `CustomerClient` (criar, listar, buscar cartões); HTTP direto para delete
- **Cache**: Redis via `ICacheService` — chave `customer-cards:{customerId}`
- **UnitOfWork**: Persiste `CustomerId` no `User` ao criar Customer no MP
- **SubscriptionRepository**: Consultado para proteger o cartão da assinatura ativa
