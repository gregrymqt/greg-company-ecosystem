using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// Certifique-se de que o namespace da EntityFile esteja acessível aqui
// using MeuCrudCsharp.Domain.Models;

namespace MeuCrudCsharp.Models
{
    public enum VideoStatus
    {
        Processing, // 0
        Available, // 1
        Error, // 2
    }

    [Index(nameof(CourseId))]
    [Index(nameof(PublicId), IsUnique = true)]
    public class Video
    {
        [Key]
        public int Id { get; set; }

        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Required]
        public required string Title { get; set; }

        public required string Description { get; set; }

        public required string StorageIdentifier { get; set; }

        public DateTime UploadDate { get; set; }

        public TimeSpan Duration { get; set; }

        public VideoStatus Status { get; set; }

        // --- RELACIONAMENTOS ---

        // 1. Relacionamento com Curso
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        // MUDANÇA AQUI: Remova 'required' e adicione '?'
        // O EF Core vai usar o CourseId para fazer o vínculo na hora de salvar.
        public virtual Course? Course { get; set; } 

        // 2. Relacionamento com Arquivo
        public int FileId { get; set; }

        [ForeignKey("FileId")]
        // MUDANÇA AQUI: Remova 'required' e adicione '?'
        public virtual EntityFile? File { get; set; } 

        // -----------------------

        [MaxLength(2048)]
        public string? ThumbnailUrl { get; set; }

        public Video()
        {
            UploadDate = DateTime.UtcNow;
            Status = VideoStatus.Processing;
        }
    }
}