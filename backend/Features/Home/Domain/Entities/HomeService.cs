using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.Home.Domain.Entities;

public class HomeServiceEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Icon { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CtaText { get; set; } = string.Empty;

    public string CtaLink { get; set; } = string.Empty;

    public string? Audience { get; set; }

    public int Order { get; set; }
}
