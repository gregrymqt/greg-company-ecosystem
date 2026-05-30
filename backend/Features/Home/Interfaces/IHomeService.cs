using MeuCrudCsharp.Features.Home.DTOs;

namespace MeuCrudCsharp.Features.Home.Interfaces;

public interface IHomeService
{
    Task<HomeContentDto> GetHomeContentAsync();

    Task<HeroSlideDto?> CreateHeroAsync(CreateUpdateHeroDto dto);

    Task<bool> UpdateHeroAsync(int id, CreateUpdateHeroDto dto);

    Task DeleteHeroAsync(int id);

    Task<ServiceDto> CreateServiceAsync(CreateUpdateServiceDto dto);
    Task UpdateServiceAsync(int id, CreateUpdateServiceDto dto);
    Task DeleteServiceAsync(int id);
}
