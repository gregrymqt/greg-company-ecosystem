using System;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Features.Home.Domain.Entities;

namespace MeuCrudCsharp.Features.Home.Domain.Interfaces;

public interface IHomeRepository
{
    Task<List<HomeHero>> GetAllHeroesAsync();
    Task<HomeHero?> GetHeroByIdAsync(string id);
    Task AddHeroAsync(HomeHero hero);
    Task UpdateHeroAsync(HomeHero hero);
    Task DeleteHeroAsync(HomeHero hero);

    Task<List<HomeServiceEntry>> GetAllServicesAsync();
    Task<HomeServiceEntry?> GetServiceByIdAsync(string id);
    Task AddServiceAsync(HomeServiceEntry service);
    Task UpdateServiceAsync(HomeServiceEntry service);
    Task DeleteServiceAsync(HomeServiceEntry service);
}

