using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Features.Videos.Domain.Entities;

public class Video
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string StorageIdentifier { get; set; }

    public DateTime UploadDate { get; set; }

    public TimeSpan Duration { get; set; }

    public VideoStatus Status { get; set; }

    // --- RELACIONAMENTOS ---

    // 1. Relacionamento com Curso
    public string CourseId { get; set; }
    public virtual Course? Course { get; set; }

    // 2. Relacionamento com Arquivo
    public string FileId { get; set; }
    public virtual EntityFile? File { get; set; }

    // -----------------------

    public string? ThumbnailUrl { get; set; }

    public Video()
    {
        UploadDate = DateTime.UtcNow;
        Status = VideoStatus.Processing;
    }
}




