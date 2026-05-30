namespace MeuCrudCsharp.Features.About.Domain.Entities;

public class TeamMember
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty; // Ref: TeamMemberDto [cite: 30]

    public string Role { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty; // URL vinda do upload

    public int? FileId { get; set; } // FK opcional para rastrear o arquivo

    public string? LinkedinUrl { get; set; } // Nullable [cite: 33]

    public string? GithubUrl { get; set; } // Nullable [cite: 34]
}
