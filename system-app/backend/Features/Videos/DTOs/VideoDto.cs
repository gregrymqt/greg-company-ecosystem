using System;
using System.ComponentModel.DataAnnotations;
using MeuCrudCsharp.Features.Files.DTOs;

namespace MeuCrudCsharp.Features.Videos.DTOs
{
    public class VideoDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? StorageIdentifier { get; set; }
        public DateTime UploadDate { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Status { get; set; }
        public string? CourseName { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class UpdateVideoDto
    {
        [Required(ErrorMessage = "The title is required.")]
        [StringLength(200, ErrorMessage = "The title cannot be longer than 200 characters.")]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public IFormFile? ThumbnailFile { get; set; }
    }

    public record PaginatedResultDto<T>(List<T> Items, int TotalCount, int Page, int PageSize);

    public class CreateVideoDto : BaseUploadDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        public IFormFile? ThumbnailFile { get; set; }
    }
}
