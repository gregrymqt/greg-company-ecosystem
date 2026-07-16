using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Home.Domain.Entities;
using MeuCrudCsharp.Features.Home.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Home.Infrastructure.Persistence.Repositories;

public class HomeRepository : IHomeRepository
{
    private readonly ApplicationDbContext _context;

    public HomeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HomeHero>> GetAllHeroesAsync()
    {
        return await _context.HomeHeroes.ToListAsync();
    }

    public async Task<HomeHero?> GetHeroByIdAsync(Guid id)
    {
        return await _context.HomeHeroes.FindAsync(id);
    }

    public async Task AddHeroAsync(HomeHero hero)
    {
        await _context.HomeHeroes.AddAsync(hero);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateHeroAsync(HomeHero hero)
    {
        _context.HomeHeroes.Update(hero);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteHeroAsync(HomeHero hero)
    {
        _context.HomeHeroes.Remove(hero);
        await _context.SaveChangesAsync();
    }

    public async Task<List<HomeServiceEntry>> GetAllServicesAsync()
    {
        return await _context.HomeServices.ToListAsync();
    }

    public async Task<HomeServiceEntry?> GetServiceByIdAsync(Guid id)
    {
        return await _context.HomeServices.FindAsync(id);
    }

    public async Task AddServiceAsync(HomeServiceEntry service)
    {
        await _context.HomeServices.AddAsync(service);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateServiceAsync(HomeServiceEntry service)
    {
        _context.HomeServices.Update(service);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(HomeServiceEntry service)
    {
        _context.HomeServices.Remove(service);
        await _context.SaveChangesAsync();
    }
}
