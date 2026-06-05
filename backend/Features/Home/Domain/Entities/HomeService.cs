namespace MeuCrudCsharp.Features.Home.Domain.Entities;

public class HomeServiceEntry
{
    public int Id { get; set; }

    public string IconClass { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ActionText { get; set; } = string.Empty;

    public string ActionUrl { get; set; } = string.Empty;
}
