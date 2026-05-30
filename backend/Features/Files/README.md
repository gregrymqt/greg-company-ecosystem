# Files Feature

Provides file management infrastructure: upload handling (direct and chunked), atomic disk+database persistence, and replacement/deletion with rollback support.

---

## Folder Structure

```
Features/Files/
├── Attributes/
│   └── AllowLargeFileAttribute.cs   # Filter attribute to raise request body size limits
├── DTOs/
│   └── BaseUploadDto.cs             # Base DTO for upload endpoints (direct or chunked)
├── Interfaces/
│   ├── IFileRepository.cs           # Data access contract
│   └── IFileService.cs              # Business logic contract
├── Repositories/
│   └── FileRepository.cs            # EF Core implementation (no SaveChanges — defers to UnitOfWork)
├── Services/
│   └── FileService.cs               # Core file logic: chunking, saving, replacing, deleting
└── README.md
```

---

## Components

### `AllowLargeFileAttribute`

Action/controller filter that overrides Kestrel and ASP.NET form size limits for large file uploads.

- Default limit: **500 MB** (configurable via constructor parameter, max 5 GB)
- Sets `IHttpMaxRequestBodySizeFeature.MaxRequestBodySize`, `MultipartBodyLengthLimit`, `ValueLengthLimit`

**Usage:**
```csharp
[AllowLargeFile(1024)] // 1 GB
[HttpPost("upload")]
public async Task<IActionResult> Upload(...)
```

### `BaseUploadDto`

Base DTO shared by upload endpoints. Supports both direct uploads and chunked uploads.

| Field | Type | Purpose |
|---|---|---|
| `File` | `IFormFile?` | The uploaded file |
| `IsChunk` | `bool` | Whether this request is a chunk |
| `ChunkIndex` | `int` | Zero-based index of the current chunk |
| `TotalChunks` | `int` | Total number of chunks expected |
| `FileName` | `string?` | Original file name for chunk reassembly |

### `FileRepository`

Wraps `ApiDbContext.Files`. All write operations (`AddAsync`, `UpdateAsync`, `DeleteAsync`) do **not** call `SaveChangesAsync` — callers must commit via `IUnitOfWork`.

### `FileService`

Implements all file operations with **atomic rollback**: if the database operation fails after a file is written to disk, the file is removed; if a replacement fails after the original is deleted, the original is restored from an in-memory backup.

| Method | Description |
|---|---|
| `ProcessChunkAsync` | Appends a chunk to a temp file; returns the final path when all chunks are received |
| `SalvarArquivoDoTempAsync` | Moves a completed temp file to `wwwroot/uploads/{categoria}/` and persists metadata |
| `SubstituirArquivoDoTempAsync` | Replaces an existing file using a completed temp file; updates database record |
| `SalvarArquivoAsync` | Streams an `IFormFile` directly to disk and persists metadata |
| `SubstituirArquivoAsync` | Replaces an existing file with a new `IFormFile`; restores old file on failure |
| `DeletarArquivoAsync` | Deletes the physical file and removes the database record |

---

## File Storage Layout

Files are stored under `wwwroot/uploads/{featureCategoria}/{guid}_{originalName}`.  
The `CaminhoRelativo` stored in the database uses forward slashes and is relative to `wwwroot`.

---

## Error Handling

- Empty file or null → `AppServiceException` (400)
- File not found in DB → `ResourceNotFoundException` (404)
- Any failure after disk write → rollback (physical file deleted or restored) before re-throwing
