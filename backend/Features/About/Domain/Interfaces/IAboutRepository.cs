using System;
using MeuCrudCsharp.Features.About.Domain.Entities;

namespace MeuCrudCsharp.Features.About.Domain.Interfaces;

public interface IAboutRepository
{
    Task<List<AboutSection>> GetAllSectionsAsync();
    Task<AboutSection?> GetSectionByIdAsync(int id);
    Task AddSectionAsync(AboutSection section);
    Task UpdateSectionAsync(AboutSection section);
    Task DeleteSectionAsync(AboutSection section);

    Task<List<TeamMember>> GetAllTeamMembersAsync();
    Task<TeamMember?> GetTeamMemberByIdAsync(int id);
    Task AddTeamMemberAsync(TeamMember member);
    Task UpdateTeamMemberAsync(TeamMember member);
    Task DeleteTeamMemberAsync(TeamMember member);
}
