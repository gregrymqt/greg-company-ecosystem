using MeuCrudCsharp.Features.Home.Application.DTOs;

namespace MeuCrudCsharp.Features.Home.Application.Interfaces;

public interface IHomeService
{
    Task<HomeContentDto> GetHomeContentAsync();

    Task<HeroSlideDto?> CreateHeroAsync(CreateUpdateHeroDto dto);

    Task<bool> UpdateHeroAsync(string id, CreateUpdateHeroDto dto);

    Task DeleteHeroAsync(string id);

    Task<ServiceDto> CreateServiceAsync(CreateUpdateServiceDto dto);
    Task UpdateServiceAsync(string id, CreateUpdateServiceDto dto);
    Task DeleteServiceAsync(string id);
}

