using MeuCrudCsharp.Features.Home.Application.DTOs;

namespace MeuCrudCsharp.Features.Home.Application.Interfaces;

public interface IHomeService
{
    Task<HomeContentDto> GetHomeContentAsync();

    Task<HeroSlideDto?> CreateHeroAsync(CreateUpdateHeroDto dto);

    Task<bool> UpdateHeroAsync(Guid id, CreateUpdateHeroDto dto);

    Task DeleteHeroAsync(Guid id);

    Task<ServiceDto> CreateServiceAsync(CreateUpdateServiceDto dto);
    Task UpdateServiceAsync(Guid id, CreateUpdateServiceDto dto);
    Task DeleteServiceAsync(Guid id);
}

