using System.Threading.Tasks;
using MeuCrudCsharp.Features.About.DTOs;

namespace MeuCrudCsharp.Features.About.Interfaces;

public interface IAboutService
{
    Task<AboutPageContentDto> GetAboutPageContentAsync();

    Task<AboutSectionDto?> CreateSectionAsync(CreateUpdateAboutSectionDto dto);
    Task<bool> UpdateSectionAsync(int id, CreateUpdateAboutSectionDto dto);
    Task DeleteSectionAsync(int id);

    Task<TeamMemberDto?> CreateTeamMemberAsync(CreateUpdateTeamMemberDto dto);
    Task<bool> UpdateTeamMemberAsync(int id, CreateUpdateTeamMemberDto dto);
    Task DeleteTeamMemberAsync(int id);
}