using System;

namespace MeuCrudCsharp.Features.Files.DTOs;

public class BaseUploadDto
{
    public IFormFile? File { get; set; }
    public bool IsChunk { get; set; } = false;
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
    public string? FileName { get; set; }
}
