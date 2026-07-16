using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Features.Videos.Domain.Entities;

public class Video : IMongoDocument
{
    public static string CollectionName => "videos";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [MongoIndex]
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Description { get; set; }

    [MongoIndex]
    public required string StorageIdentifier { get; set; }

    public DateTime UploadDate { get; set; }
    
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; }

    public TimeSpan Duration { get; set; }

    public VideoStatus Status { get; set; }
    
    public string? RawVideoUrl { get; set; }
    public string? StreamingUrl { get; set; }

    // --- RELACIONAMENTOS ---

    // 1. Relacionamento com Curso
    public string CourseId { get; set; }
    
    [BsonIgnore]
    public virtual Course? Course { get; set; }

    // 2. Relacionamento com Arquivo
    public string FileId { get; set; }

    [BsonIgnore]
    public virtual EntityFile? File { get; set; }

    // -----------------------

    public string? ThumbnailUrl { get; set; }

    public Video()
    {
        UploadDate = DateTime.UtcNow;
        DateCreated = DateTime.UtcNow;
        LastUpdated = DateTime.UtcNow;
        Status = VideoStatus.Pending;
    }
}




