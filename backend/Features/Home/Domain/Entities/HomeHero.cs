using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.Home.Domain.Entities;

public class HomeHero
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string? FileId { get; set; }

    public string CtaText { get; set; } = string.Empty;

    public string CtaLink { get; set; } = string.Empty;

    public string? Audience { get; set; }

    public int Order { get; set; }
}
