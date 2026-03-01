# Videos

Handles video upload, HLS processing via FFmpeg, and streaming to authenticated users.

## Responsibilities

- Accept chunked or single-file video uploads
- Persist video metadata and schedule background HLS conversion via Hangfire
- Serve HLS manifest (`.m3u8`) and segment (`.ts`) files for playback
- Notify progress of background processing in real-time via SignalR
- Admin CRUD: list, update, delete videos with cache invalidation

## Structure

| Layer | File | Role |
|---|---|---|
| Controllers | `AdminVideosController` | Admin upload, list, update, delete |
| Controllers | `VideosController` | HLS manifest and segment serving for authenticated users |
| Service | `AdminVideoService` | Upload logic, chunking, entity persistence, Hangfire scheduling |
| Service | `VideoProcessingService` | FFmpeg HLS conversion, progress notification, status updates |
| Service | `ProgressRunnerService` | Generic process runner with real-time stderr streaming |
| Notification | `VideoNotificationService` | SignalR progress push via `ConnectionMapping` |
| Repository | `VideoRepository` | EF Core data access (UnitOfWork pattern) |
| Interfaces | `IAdminVideoService`, `IVideoProcessingService`, `IProcessRunnerService`, `IVideoNotificationService`, `IVideoRepository` | Contracts for each layer |
| DTOs | `VideoDto`, `CreateVideoDto`, `UpdateVideoDto`, `PaginatedResultDto<T>` | Request/response models |
| Utils | `VideoMapper` | Entity → DTO mapping |
| Utils | `VideoDirectoryHelper` | Deletes HLS folder on video removal |

## Endpoints

### Admin (`/api/admin/videos`, requires `Admin` role)

| Method | Route | Description |
|---|---|---|
| POST | `/api/admin/videos` | Upload video (chunked or single, max 3 GB) |
| GET | `/api/admin/videos?page=1&pageSize=10` | Paginated video list |
| PUT | `/api/admin/videos/{id}` | Update title, description, thumbnail (max 1 GB) |
| DELETE | `/api/admin/videos/{id}` | Delete video record and physical files |

### Public (`/api/videos`, requires authentication)

| Method | Route | Description |
|---|---|---|
| GET | `/api/videos/{storageIdentifier}/manifest.m3u8` | HLS manifest for a video |
| GET | `/api/videos/{storageIdentifier}/hls/{segmentName}` | HLS segment file |

## Key Patterns

- **Chunked upload**: `CreateVideoDto` extends `BaseUploadDto`; `ProcessChunkAsync` reassembles chunks into a temp file before saving.
- **UnitOfWork**: all repository writes are in-memory; service calls `CommitAsync()` as a single atomic commit.
- **Background processing**: after upload commit, `IBackgroundJobClient.Enqueue` schedules `VideoProcessingService.ProcessVideoToHlsAsync` in Hangfire.
- **FFmpeg pipeline**: `VideoProcessingService` builds the HLS conversion command, streams stderr via `ProgressRunnerService`, parses `time=` lines with a compiled Regex, and reports 0–99% progress to SignalR clients.
- **Compensating delete**: if physical file deletion fails after a successful DB delete, the error is logged but no exception is thrown (partial success).
- **Cache versioning**: `GetAllVideosAsync` uses a cache version key (`videos_cache_version`) invalidated on every update/delete.
