using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MeuCrudCsharp.Features.Videos.Application.DTOs;

namespace MeuCrudCsharp.Features.Courses.Application.DTOs
{
    public class CourseDto
    {
        public Guid PublicId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Year { get; set; }
        public string? Creator { get; set; }
        public decimal Price { get; set; }
        public bool IsPublished { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<ModuleDto> Modules { get; set; } = new();
    }

    public class ModuleDto
    {
        public Guid PublicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<LessonDto> Lessons { get; set; } = new();
    }

    public class LessonDto
    {
        public Guid PublicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public Guid VideoPublicId { get; set; }
        public string VideoTitle { get; set; } = string.Empty;
    }

    public class CreateUpdateCourseDto
    {

        [Required(ErrorMessage = "O nome do curso e obrigatorio.")]
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(4)]
        public string? Year { get; set; }

        [StringLength(100)]
        public string? Creator { get; set; }

        public decimal Price { get; set; } = 0;

        public bool IsPublished { get; set; } = false;

        public string? ThumbnailUrl { get; set; }

        public List<CreateUpdateModuleDto>? Modules { get; set; }
    }

    public class CreateUpdateModuleDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<CreateUpdateLessonDto> Lessons { get; set; } = new();
    }

    public class CreateUpdateLessonDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public Guid VideoPublicId { get; set; }
        public string VideoTitle { get; set; } = string.Empty;
    }
}
