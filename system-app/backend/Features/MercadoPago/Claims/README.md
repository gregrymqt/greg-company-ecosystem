# MercadoPago — Claims Feature

Gerenciamento de reclamações pós-compra integrado à API do Mercado Pago.

## Responsabilidade

Permite que administradores e usuários visualizem, respondam e escalem reclamações abertas no Mercado Pago (disputas de pagamento).

## Estrutura

```
Claims/
├── Controllers/
│   ├── AdminClaimsController.cs   # Endpoints administrativos (inbox, detalhes, resposta)
│   └── UserClaimsController.cs    # Endpoints do aluno (suas reclamações, resposta, mediação)
├── DTOs/
│   └── MercadoPagoClaimsDTOs.cs   # Contratos de dados da API do Mercado Pago
├── Interfaces/
│   ├── IAdminClaimService.cs
│   ├── IClaimRepository.cs
│   └── IMercadoPagoIntegrationService.cs
├── Repositories/
│   └── ClaimRepository.cs         # Acesso ao banco via EF Core
├── Services/
│   ├── AdminClaimService.cs       # Lógica administrativa (listagem, detalhes, resposta)
│   ├── MercadoPagoIntegrationService.cs  # Cliente HTTP para a API do Mercado Pago
│   └── UserClaimService.cs        # Lógica do aluno (validação de posse, resposta, mediação)
├── ViewModels/
│   └── MercadoPagoClaimsViewModels.cs  # DTOs de resposta para o frontend
└── Models/                        # Entidade Claims (persiste no banco local)
```

## Endpoints

### Admin (`/api/admin/claims`)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/` | Lista paginada de reclamações (inbox) com filtros por termo e status |
| GET | `/{id}` | Detalhes de uma reclamação com histórico de mensagens em tempo real |
| POST | `/{id}/reply` | Envia resposta do administrador para o Mercado Pago |

### User (`/api/user/claims`)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/` | Lista as reclamações do usuário logado |
| GET | `/{id}` | Detalhes de uma reclamação (validado por posse) |
| POST | `/{id}/reply` | Aluno envia resposta |
| POST | `/{id}/mediation` | Aluno solicita mediação do Mercado Pago |

## Integrações

- **Mercado Pago API**: `post-purchase/v1/claims` — busca, mensagens, envio e mediação
- **Banco de dados local**: Tabela `Claims` sincronizada via webhook (processado pelo `ChargebackJob`)
- **Autenticação**: JWT obrigatório; endpoints de usuário validam posse da reclamação
