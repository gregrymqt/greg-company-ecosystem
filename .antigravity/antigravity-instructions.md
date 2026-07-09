# Greg Company Ecosystem - AI Agent Instructions

## Project Architecture

This is a **monorepo multi-service business suite** following a decoupled micro-frontend architecture.

1. **Proxy Gateway (Nginx):** Acts as the sole entry point to the ecosystem (Port 80) and routes traffic to the respective services.
2. **Backend (.NET 8):** Primary transactional API named `MeuCrudCsharp`. Handles logic, RabbitMQ messaging, Transactional Outbox Pattern, and Business Intelligence metrics.
3. **Go Worker (Golang):** Microservice dedicated to video transcoding, relieving the C# backend from heavy local file system I/O.
4. **Ecommerce Bot (Python):** Python automation project located in `ecommerce-bot/`.
5. **Portal Frontend (React):** Facing application for end-users, students, and course consumers.
6. **Admin Frontend (React):** Backoffice management, analytics dashboards, and refund workflows.
7. **Infra:** Centralized infrastructure orchestration directory containing docker-compose and proxy configs.

## Backend (C# / .NET 8)

### Architecture Pattern: Clean Architecture with Vertical Slice
- **Features-first organization:** All features live under `backend/Features/` directory (e.g., Auth, Courses, Support, MercadoPago, Mcp, etc.).
- Each feature contains its own: Controllers, Services, Repositories, DTOs, Interfaces, Mappers.
- **Extension-based startup:** `Program.cs` uses extension methods from `Extensions/` for clean dependency registration and app pipeline setup.
- **Transactional Outbox:** To prevent distributed transaction failures, asynchronous events (like emails and status changes) are saved to `OutboxEvents` within the same transaction as the database operation.

### Dependency Injection Convention
Services and repositories are **auto-registered via Scrutor** by namespace scanning. When adding new features, create `Services/` and `Repositories/` folders strictly following the standard namespace pattern so they will be automatically discovered.

### Database & Background Jobs
- **Primary DB:** MongoDB via native `MongoDB.Driver`. It serves as the single source of truth.
- **Cache:** Redis for performance (`USE_REDIS` env var toggles it).
- **Messaging:** RabbitMQ is used as the AMQP broker.
- **Storage:** Supabase Storage is used for files.

## Microservices and Bots

### Go Worker (Video Transcoding)
The `go-worker` directory contains the logic for processing and transcoding videos asynchronously. The C# backend pushes tasks to RabbitMQ, which are consumed by the Go worker.

### Ecommerce Bot (Python)
The `ecommerce-bot` directory houses the Python automation scripts and services. Make sure `venv` is excluded from source control.

## Frontends (React + TypeScript + Vite)

### Micro-frontend Separation
The application UI is split into two completely independent React projects:
1. **`portal/`**: For end-users.
2. **`admin/`**: For administrators.

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

## Docker Orchestration

### Infrastructure Layout
- **Global Stack:** Production orchestration lives in `infra/docker-compose.yml`. This creates the `greg-network` and spins up MongoDB, Redis, RabbitMQ, the Backend, both Frontends, the Go Worker, and the Nginx `proxy-gateway`.
- **Nginx Config:** Located at `infra/nginx.conf`. 
- **Local Stacks:** Local dev configs reside inside each project folder (e.g., `backend/docker-compose.yml`).
- **Testing Stack:** Automated tests orchestration is located at `backend/docker-compose.test.yml`.

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
3. Ensure it aligns with `MeuCrudCsharp.csproj` structure.

### Frontend Feature
1. Determine if the feature belongs to `portal/`, `admin/`, or both.
2. Create folder under `src/features/{featurename}/` in the target application(s).
3. Do not cross-import. Ensure features use their isolated routing and contexts.

## Key Files Reference
- Backend entry: [backend/Program.cs](backend/Program.cs)
- Backend csproj: [backend/MeuCrudCsharp.csproj](backend/MeuCrudCsharp.csproj)
- Portal entry: [portal/src/main.tsx](portal/src/main.tsx)
- Admin entry: [admin/src/main.tsx](admin/src/main.tsx)
- Infra config: [infra/docker-compose.yml](infra/docker-compose.yml)
