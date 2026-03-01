# Profiles / Admin

Administrative read-only access to student profiles, including subscription and plan data.

## Responsibilities

- List all students with pagination
- Fetch a single student by their public ID
- Cache paginated results to reduce database load

## Structure

| Layer | File | Role |
|---|---|---|
| Controller | `AdminStudentsController` | Admin REST endpoints for student listing and detail |
| Service | `AdminStudentService` | Business logic, cache management, entity-to-DTO mapping |
| Repository | `StudentRepository` | EF Core queries with `Include` on Subscription and Plan |
| Interface | `IAdminStudentService` | Contract for the admin service |
| Interface | `IStudentRepository` | Contract for the repository |
| DTO | `StudentDto` | Read-only record projected from `Users` + `Subscription` + `Plan` |

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/admin/students?page=1&pageSize=10` | Paginated list of all students |
| GET | `/api/admin/students/{id}` | Single student by public GUID |

All endpoints require the `Admin` role.

## Key Patterns

- **Pagination**: `GetAllWithSubscriptionsAsync` returns a `(Items, TotalCount)` tuple; the service wraps it in `PaginatedResult<StudentDto>`.
- **Cache**: Results cached per `page`/`pageSize` combination for 5 minutes via `ICacheService`.
- **Read-only queries**: Repository uses `AsNoTracking()` throughout — no write operations in this slice.
