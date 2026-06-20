using System;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Home.Domain.Entities;
using MeuCrudCsharp.Features.Home.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.Home.Infrastructure.Persistence.Repositories;

public class HomeRepository : IHomeRepository
{
    private readonly IMongoCollection<HomeHero> _homeHeroes;
    private readonly IMongoCollection<HomeServiceEntry> _homeServices;

    public HomeRepository(IMongoDbContext context)
    {
        _homeHeroes = context.GetCollection<HomeHero>("home_heroes");
        _homeServices = context.GetCollection<HomeServiceEntry>("home_services");
    }

    public async Task<List<HomeHero>> GetAllHeroesAsync()
    {
        return await _homeHeroes.Find(FilterDefinition<HomeHero>.Empty).ToListAsync();
    }

    public async Task<HomeHero?> GetHeroByIdAsync(string id)
    {
        return await _homeHeroes.Find(h => h.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddHeroAsync(HomeHero hero)
    {
        await _homeHeroes.InsertOneAsync(hero);
    }

    public async Task UpdateHeroAsync(HomeHero hero)
    {
        await _homeHeroes.ReplaceOneAsync(h => h.Id == hero.Id, hero);
    }

    public async Task DeleteHeroAsync(HomeHero hero)
    {
        await _homeHeroes.DeleteOneAsync(h => h.Id == hero.Id);
    }

    public async Task<List<HomeServiceEntry>> GetAllServicesAsync()
    {
        return await _homeServices.Find(FilterDefinition<HomeServiceEntry>.Empty).ToListAsync();
    }

    public async Task<HomeServiceEntry?> GetServiceByIdAsync(string id)
    {
        return await _homeServices.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddServiceAsync(HomeServiceEntry service)
    {
        await _homeServices.InsertOneAsync(service);
    }

    public async Task UpdateServiceAsync(HomeServiceEntry service)
    {
        await _homeServices.ReplaceOneAsync(s => s.Id == service.Id, service);
    }

    public async Task DeleteServiceAsync(HomeServiceEntry service)
    {
        await _homeServices.DeleteOneAsync(s => s.Id == service.Id);
    }
}
