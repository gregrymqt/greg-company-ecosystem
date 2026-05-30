# Support

Manages customer support tickets stored in MongoDB.

## Responsibilities

- Allow authenticated users to open support tickets
- Allow admins to list, view, and update ticket status
- Map MongoDB documents to response DTOs via `SupportMapper`

## Structure

| Layer | File | Role |
|---|---|---|
| Controller | `SupportController` | REST endpoints for ticket creation, listing, detail, and status update |
| Service | `SupportService` | Business logic, validation, orchestration |
| Repository | `SupportRepository` | MongoDB CRUD via `IMongoCollection<SupportTicketDocument>` |
| Interface | `ISupportService` | Contract for the service |
| Interface | `ISupportRepository` | Contract for the repository |
| DTO | `SupportDTO` | `CreateSupportTicketDto`, `UpdateSupportTicketDto`, `SupportTicketResponseDto` |
| Utils | `SupportMapper` | Maps `SupportTicketDocument` → `SupportTicketResponseDto` |

## Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/support` | Required | Create a new support ticket |
| GET | `/api/support?page=1&pageSize=10` | Admin | Paginated list of all tickets |
| GET | `/api/support/{id}` | Admin | Get a single ticket by ID |
| PUT | `/api/support/{id}` | Admin | Update ticket status |

## Key Patterns

- **MongoDB storage**: tickets are documents (`SupportTicketDocument`); collection name is resolved from the static `CollectionName` property on the model.
- **Pagination**: repository returns `(Data, Total)` tuple; service wraps it in `PaginatedResultDto<SupportTicketResponseDto>`.
- **`ResourceNotFoundException`**: thrown by service when a ticket is not found; `HandleException` in the base controller maps it to `404`.
