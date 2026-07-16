using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Courses.Domain.Entities;

public class Course
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Year { get; set; }

    public string? Creator { get; set; }

    public decimal Price { get; set; } = 0;

    public bool IsPublished { get; set; } = false;

    public string? ThumbnailUrl { get; set; }

    public List<Module> Modules { get; set; } = new();
}
