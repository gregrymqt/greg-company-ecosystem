using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Domain.Entities;

public class Video
{
    public int Id { get; set; }

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string StorageIdentifier { get; set; }

    public DateTime UploadDate { get; set; }

    public TimeSpan Duration { get; set; }

    public VideoStatus Status { get; set; }

    // --- RELACIONAMENTOS ---

    // 1. Relacionamento com Curso
    public int CourseId { get; set; }
    public virtual Course? Course { get; set; }

    // 2. Relacionamento com Arquivo
    public int FileId { get; set; }
    public virtual EntityFile? File { get; set; }

    // -----------------------

    public string? ThumbnailUrl { get; set; }

    public Video()
    {
        UploadDate = DateTime.UtcNow;
        Status = VideoStatus.Processing;
    }
}
