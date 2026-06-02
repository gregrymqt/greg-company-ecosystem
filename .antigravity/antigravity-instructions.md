# Greg Company Ecosystem - AI Agent Instructions

## Project Architecture

This is a **monorepo multi-service business suite** following a decoupled micro-frontend architecture.

1. **Backend (.NET 8):** Primary transactional API, background job processing (Hangfire), and Business Intelligence metrics calculation.
2. **Portal Frontend (React):** Facing application for end-users, students, and course consumers.
3. **Admin Frontend (React):** Backoffice management, analytics dashboards, and refund workflows.
4. **Infra:** Centralized infrastructure orchestration directory.

## Backend (C# / .NET 8)

### Architecture Pattern: Clean Architecture with Vertical Slice
- **Features-first organization:** All features live under `backend/Features/` directory (Auth, Courses, Analytics, Support, etc.)
- Each feature contains its own: Controllers, Services, Repositories, DTOs, Interfaces, Mappers.
- **Extension-based startup:** `Program.cs` uses extension methods from `Extensions/` for clean dependency registration.

### Dependency Injection Convention
Services and repositories are **auto-registered via Scrutor** by namespace scanning. When adding new features, create `Services/` and `Repositories/` folders strictly following the standard namespace pattern so they will be automatically discovered.

### Database & Background Jobs
- **Primary DB:** SQL Server via EF Core. Migrations are managed from the `backend/` directory.
- **Cache:** Redis for performance (`USE_REDIS` env var toggles it).
- **NoSQL:** MongoDB for flexible document storage (e.g., Support tickets).
- **Hangfire:** Handles all async processing (subscription renewals, MercadoPago webhooks).

## Frontends (React + TypeScript + Vite)

### Micro-frontend Separation
The application UI is split into two completely independent React projects to avoid coupling and cross-environment bleeding:
1. **`portal/`**: For end-users. Runs on port `5173`.
2. **`admin/`**: For administrators. Runs on port `5174`.

### Project Structure (Inside portal/ or admin/)
```
src/
  features/    - Feature-based components (mirrors backend features)
  components/  - Shared/reusable UI components (Sidebar, Layouts)
  pages/       - Route-level page components
  routes/      - Routing configuration
  shared/      - Shared utilities, types, constants
```
*Note: Features in these folders are flattened. Do not use generic `Public/` or `Admin/` subfolders inside a feature. Place the code directly in the relevant micro-frontend's feature folder.*

### UI & Real-time Context
- **WebSockets:** Uses SignalR (`@microsoft/signalr`) for live payment confirmations and analytics updates.
- Components use `AppHubsCSharp` to connect to appropriate backend hubs.

## Docker Orchestration

### Infrastructure Layout
- **Global Stack:** Production orchestration lives in `infra/docker-compose.yml` (It orchestrates Backend, DBs, and the Frontends built on Nginx).
- **Local Stacks:** Local dev configs reside inside each project folder (e.g., `backend/docker-compose.yml`).
- **Testing Stack:** The automated tests orchestration is located at `backend/docker-compose.test.yml`.

## CI/CD Pipelines (GitHub Actions)
The pipelines are fully decoupled and trigger based on path filters (`.github/workflows/`):
- `ci-cd-backend.yml` (monitors `backend/**` and `Tests/**`)
- `ci-cd-portal.yml` (monitors `portal/**`)
- `ci-cd-admin.yml` (monitors `admin/**`)
Images are published to GHCR using both `latest` and `${{ github.sha }}` tagging strategies.

## Adding New Features

### Backend Feature
1. Create folder under `backend/Features/{FeatureName}/`
2. Add subfolders: `Controllers/`, `Services/`, `Repositories/`, `DTOs/`, `Interfaces/`.
3. Add DbSet to `backend/Data/ApiDbContext.cs` if it introduces a new entity.
4. Create migration by running EF core CLI from the `backend/` directory.

### Frontend Feature
1. Determine if the feature belongs to `portal/`, `admin/`, or both (e.g., `Payment` goes to both).
2. Create folder under `src/features/{featurename}/` in the target application(s).
3. Do not cross-import. Ensure features use their isolated routing and contexts.

## Key Files Reference
- Backend entry: [backend/Program.cs](backend/Program.cs)
- DB context: [backend/Data/ApiDbContext.cs](backend/Data/ApiDbContext.cs)
- Portal entry: [portal/src/main.tsx](portal/src/main.tsx)
- Admin entry: [admin/src/main.tsx](admin/src/main.tsx)
- Infra config: [infra/docker-compose.yml](infra/docker-compose.yml)
