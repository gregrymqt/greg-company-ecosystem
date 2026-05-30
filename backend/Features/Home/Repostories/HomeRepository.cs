using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Home.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Home.Repostories;

public class HomeRepository : IHomeRepository
{
    private readonly ApiDbContext _context;

    public HomeRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<List<HomeHero>> GetAllHeroesAsync()
    {
        return await _context.HomeHeroes.AsNoTracking().ToListAsync();
    }

    public async Task<HomeHero?> GetHeroByIdAsync(int id)
    {
        return await _context.HomeHeroes.FindAsync(id);
    }

    public async Task AddHeroAsync(HomeHero hero)
    {
        await _context.HomeHeroes.AddAsync(hero);
    }

    public Task UpdateHeroAsync(HomeHero hero)
    {
        _context.HomeHeroes.Update(hero);
        return Task.CompletedTask;
    }

    public Task DeleteHeroAsync(HomeHero hero)
    {
        _context.HomeHeroes.Remove(hero);
        return Task.CompletedTask;
    }

    public async Task<List<HomeService>> GetAllServicesAsync()
    {
        return await _context.HomeServices.AsNoTracking().ToListAsync();
    }

    public async Task<HomeService?> GetServiceByIdAsync(int id)
    {
        return await _context.HomeServices.FindAsync(id);
    }

    public async Task AddServiceAsync(HomeService service)
    {
        await _context.HomeServices.AddAsync(service);
    }

    public Task UpdateServiceAsync(HomeService service)
    {
        _context.HomeServices.Update(service);
        return Task.CompletedTask;
    }

    public Task DeleteServiceAsync(HomeService service)
    {
        _context.HomeServices.Remove(service);
        return Task.CompletedTask;
    }
}
