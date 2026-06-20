using System.Threading.Tasks;
using MeuCrudCsharp.Features.About.Application.DTOs;

namespace MeuCrudCsharp.Features.About.Application.Interfaces;

public interface IAboutService
{
    Task<AboutPageContentDto> GetAboutPageContentAsync();

    Task<AboutSectionDto?> CreateSectionAsync(CreateUpdateAboutSectionDto dto);
    Task<bool> UpdateSectionAsync(string id, CreateUpdateAboutSectionDto dto);
    Task DeleteSectionAsync(string id);

    Task<TeamMemberDto?> CreateTeamMemberAsync(CreateUpdateTeamMemberDto dto);
    Task<bool> UpdateTeamMemberAsync(string id, CreateUpdateTeamMemberDto dto);
    Task DeleteTeamMemberAsync(string id);
}
