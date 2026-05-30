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
        public List<VideoDto> Videos { get; set; } = new();
    }

    public class CreateUpdateCourseDto
    {

        [Required(ErrorMessage = "O nome do curso é obrigatório.")]
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
