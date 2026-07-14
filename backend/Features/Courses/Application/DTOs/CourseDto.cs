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
        public List<VideoDto> Videos { get; set; } = new();
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
    }
}
