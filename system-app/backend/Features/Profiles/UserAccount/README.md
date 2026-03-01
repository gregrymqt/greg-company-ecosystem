# Profiles / UserAccount

Manages authenticated user account operations, currently focused on profile picture uploads.

## Responsibilities

- Upload and replace a user's avatar image
- Resolve the current user's identity via `IUserContext`
- Delegate file persistence to `IFileService` (create new or replace existing)
- Persist the updated `AvatarFileId` reference via `IUnitOfWork`

## Structure

| Layer | File | Role |
|---|---|---|
| Controller | `UserAccountController` | REST endpoint for avatar upload |
| Service | `UserAccountService` | Business logic for profile picture update |
| Repository | `UserAccountRepository` | Fetches user entity by ID |
| Interface | `IUserAccountService` | Contract for the account service |
| Interface | `IUserAccountRepository` | Contract for the repository |
| DTO | `UserProfileDto` / `AvatarUpdateResponse` | Profile data and upload response |
| ViewModel | `ProfileViewModel` | Aggregated model combining profile, subscription, and payment history |

## Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/user-account/avatar` | Required | Upload or replace the current user's profile picture (max 20 MB) |

## Key Patterns

- **Upsert logic**: if the user already has an `AvatarFileId`, calls `SubstituirArquivoAsync` (replaces); otherwise calls `SalvarArquivoAsync` (creates new).
- **UnitOfWork**: file is persisted via `_fileService`, then the user entity's `AvatarFileId` is updated and committed in a single `CommitAsync` call.
- **Identity**: current user resolved from JWT claims via `IUserContext.GetCurrentUserId()`.
