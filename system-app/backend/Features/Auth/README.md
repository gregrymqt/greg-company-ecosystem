# Feature: Auth

Gerencia todo o ciclo de autenticação da plataforma: login com email/senha, login via Google OAuth, registro de novos usuários, emissão de tokens JWT e logout com blacklist no Redis.

## Estrutura

```
Auth/
├── Controllers/
│   └── AuthController.cs          # Endpoints REST de autenticação
├── Dtos/
│   ├── LoginRequestDto.cs         # Contratos de entrada (login e registro)
│   ├── LoginResponseDto.cs        # Contrato de saída (token + sessão)
│   └── UserSessionDto.cs          # Dados do usuário autenticado
├── Interfaces/
│   ├── iAppAuthService.cs         # Contrato do serviço principal de auth
│   ├── IJwtService.cs             # Contrato de geração de JWT
│   ├── IUserContext.cs            # Contrato de acesso ao usuário da requisição atual
│   ├── IUserRepository.cs         # Contrato de acesso a dados de Users
│   └── IUserRoleRepository.cs     # Contrato de acesso a roles de Users
├── Middlewares/
│   └── JwtBlacklistMiddleware.cs  # Intercepta tokens revogados via Redis
├── Repositories/
│   ├── UserRepository.cs          # Implementação EF Core para Users
│   └── UserRoleRepository.cs      # Implementação EF Core para UserRoles
├── Services/
│   ├── AppAuthService.cs          # Regras de negócio de autenticação
│   └── JwtService.cs              # Geração e assinatura de tokens JWT
└── Utils/
    └── UserContext.cs             # Implementação de IUserContext via IHttpContextAccessor
```

## Endpoints

Base route: `/api/auth`

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/api/auth/google-login` | Anônimo | Inicia o fluxo OAuth com o Google (redirect) |
| `GET` | `/api/auth/google-callback` | Anônimo | Callback do Google, retorna JWT via fragmento de URL |
| `POST` | `/api/auth/login` | Anônimo | Login com email e senha |
| `POST` | `/api/auth/register` | Anônimo | Registro de novo usuário |
| `GET` | `/api/auth/me` | JWT | Retorna os dados da sessão do usuário autenticado |
| `POST` | `/api/auth/logout` | JWT | Invalida o token atual adicionando-o à blacklist do Redis |

## Fluxos

### Login com Email/Senha (`POST /api/auth/login`)

1. Valida email via `UserManager`
2. Valida senha via `CheckPasswordAsync`
3. Gera JWT com expiração via `JwtService`
4. Busca dados completos via `GetAuthenticatedUserDataAsync` (paralelo)
5. Retorna `LoginResponseDto` com token + sessão

### Login com Google

1. `GET /google-login` → redireciona para Google Accounts
2. Google retorna para `GET /google-callback`
3. Se o usuário não existir, cria conta e vincula o GoogleId
4. Gera JWT e redireciona o frontend para `/google-callback#token=<jwt>` (fragmento de URL, não enviado ao servidor)

### Logout (`POST /api/auth/logout`)

1. Extrai o token do header `Authorization: Bearer <token>`
2. Calcula o TTL restante do token (lendo o campo `exp`)
3. Adiciona o token na blacklist do Redis com o mesmo TTL
4. `JwtBlacklistMiddleware` bloqueia requisições futuras com esse token

## DTOs

### Entrada — Login

```json
{
  "email": "user@example.com",
  "password": "string"
}
```

### Entrada — Registro

```json
{
  "name": "string",
  "email": "user@example.com",
  "password": "string",
  "confirmPassword": "string"
}
```

### Saída — Login/Registro/Me

```json
{
  "user": {
    "publicId": "uuid",
    "name": "string",
    "email": "string",
    "avatarUrl": "string",
    "roles": ["User"],
    "hasActiveSubscription": true,
    "hasPaymentHistory": false
  },
  "token": "eyJhbGci...",
  "refreshToken": "uuid",
  "expiration": "2026-02-28T20:00:00Z"
}
```

## JWT

Configurado em `JwtSettings` (via `appsettings.json`). O token contém as claims:

| Claim | Valor |
|-------|-------|
| `sub` | `user.Id` |
| `email` | `user.Email` |
| `jti` | GUID único por token |
| `name` | `user.Name` |
| `AvatarUrl` | `user.AvatarUrl` |
| `role` | Uma ou mais roles do usuário |

Expiração padrão: **8 horas**.

## Blacklist de Tokens (Redis)

O `JwtBlacklistMiddleware` roda em toda requisição autenticada. A chave no Redis segue o padrão `blacklist:<token>` com TTL igual ao tempo restante do token, garantindo limpeza automática.

## Roles

Ao criar um usuário (via Google ou registro), `addRolesToUser` atribui automaticamente:
- `Admin` — se o email for o email admin configurado no service
- `User` — para todos os demais

## Tratamento de Erros

| Exceção | HTTP |
|---------|------|
| `UnauthorizedAccessException` | `401 Unauthorized` |
| `ResourceNotFoundException` | `404 Not Found` |
| `AppServiceException` / `InvalidOperationException` | `400 Bad Request` |
| `Exception` (genérica) | `500 Internal Server Error` |

> `HandleException` (herdado de `ApiControllerBase`) trata os casos genéricos. `UnauthorizedAccessException` é capturado explicitamente no endpoint de login por precisar retornar `401`.
