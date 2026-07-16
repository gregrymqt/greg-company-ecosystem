using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using MeuCrudCsharp.Features.Courses.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Domain.Entities;

public class Video
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string StorageIdentifier { get; set; }

    public DateTime UploadDate { get; set; }

    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; }

    public TimeSpan Duration { get; set; }

    public VideoStatus Status { get; set; }

    public string? RawVideoUrl { get; set; }
    public string? StreamingUrl { get; set; }

    public Guid? CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }

    public Guid? FileId { get; set; }

    [ForeignKey(nameof(FileId))]
    public virtual EntityFile? File { get; set; }

    public string? ThumbnailUrl { get; set; }

    public Video()
    {
        UploadDate = DateTime.UtcNow;
        DateCreated = DateTime.UtcNow;
        LastUpdated = DateTime.UtcNow;
        Status = VideoStatus.Pending;
    }
}
