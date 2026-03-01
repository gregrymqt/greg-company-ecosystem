# Home Feature

Manages the public home page content: Hero slides (with image upload/chunking) and Services cards (JSON only). Includes cache invalidation on every write operation.

---

## Folder Structure

```
Features/Home/
├── Controllers/
│   └── HomeController.cs       # API endpoints for Hero and Services
├── DTOs/
│   └── HomeDtos.cs             # Read DTOs (output) and write DTOs (input)
├── Interfaces/
│   ├── IHomeRepository.cs      # Data access contract
│   └── IHomeService.cs         # Business logic contract
├── Repostories/
│   └── HomeRepository.cs       # EF Core implementation (no SaveChanges — defers to UnitOfWork)
├── Services/
│   └── HomeService.cs          # Core logic: chunked uploads, cache management
└── README.md
```

---

## Endpoints

### Hero (supports chunked file upload)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/home` | Returns full home content (heroes + services) |
| `POST` | `/api/home/hero` | Creates a new hero slide (supports chunked upload, max 2 GB) |
| `PUT` | `/api/home/hero/{id}` | Updates an existing hero slide |
| `DELETE` | `/api/home/hero/{id}` | Deletes a hero slide and its associated file |

### Services (JSON only)

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/home/services` | Creates a new service card |
| `PUT` | `/api/home/services/{id}` | Updates an existing service card |
| `DELETE` | `/api/home/services/{id}` | Deletes a service card |

---

## DTOs

### Read (output)

- `HomeContentDto` — top-level response with `Hero` and `Services` lists
- `HeroSlideDto` — `Id`, `ImageUrl`, `Title`, `Subtitle`, `ActionText`, `ActionUrl`
- `ServiceDto` — `Id`, `IconClass`, `Title`, `Description`, `ActionText`, `ActionUrl`

### Write (input)

- `CreateUpdateHeroDto` — extends `BaseUploadDto` (chunking support) + `Title`, `Subtitle`, `ActionText`, `ActionUrl`
- `CreateUpdateServiceDto` — `IconClass`, `Title`, `Description`, `ActionText`, `ActionUrl`

---

## Chunked Upload Flow (Hero)

1. Frontend splits the file into chunks and POSTs each one with `IsChunk=true`, `ChunkIndex`, `TotalChunks`, `FileName`
2. Each intermediate chunk returns `200 OK` with a progress message
3. The final chunk triggers file assembly, moves the temp file to `wwwroot/uploads/HomeHero/`, persists to DB, and returns `201 Created`
4. On update (`PUT`), intermediate chunks return `200 OK`; the final chunk returns `204 No Content`

---

## Caching

- Cache key: `HOME_PAGE_CONTENT`
- `GetHomeContentAsync` uses `cache.GetOrCreateAsync` to avoid repeated DB queries
- All write operations (`CreateHero`, `UpdateHero`, `DeleteHero`, `CreateService`, `UpdateService`, `DeleteService`) call `cache.RemoveAsync` after committing

---

## Error Handling

All controller actions use `HandleException` from `ApiControllerBase`:
- `AppServiceException` / `InvalidOperationException` → 400
- `ResourceNotFoundException` → 404
- Unhandled exceptions → 500
