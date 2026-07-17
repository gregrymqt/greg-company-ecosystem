using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCrudCsharp.Features.Courses.Domain.Entities;

public class Lesson
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    public Guid? ModuleId { get; set; }

    [ForeignKey(nameof(ModuleId))]
    public Module? Module { get; set; }

    public Guid VideoPublicId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;

    public bool IsVideoAvailable { get; set; } = false;
}
