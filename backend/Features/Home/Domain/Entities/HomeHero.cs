namespace MeuCrudCsharp.Features.Home.Domain.Entities;

public class HomeHero
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int? FileId { get; set; }

    public string ActionText { get; set; } = string.Empty;

    public string ActionUrl { get; set; } = string.Empty;
}
