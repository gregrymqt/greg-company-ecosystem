using System;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.About.Repositories;

public class AboutRepository : IAboutRepository
{
    private readonly ApiDbContext _context;

    public AboutRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<List<AboutSection>> GetAllSectionsAsync()
    {
        return await _context.AboutSections.OrderBy(s => s.OrderIndex).AsNoTracking().ToListAsync();
    }

    public async Task<AboutSection?> GetSectionByIdAsync(int id)
    {
        return await _context.AboutSections.FindAsync(id);
    }

    public async Task AddSectionAsync(AboutSection section)
    {
        await _context.AboutSections.AddAsync(section);
    }

    public Task UpdateSectionAsync(AboutSection section)
    {
        _context.AboutSections.Update(section);
        return Task.CompletedTask;
    }

    public Task DeleteSectionAsync(AboutSection section)
    {
        _context.AboutSections.Remove(section);
        return Task.CompletedTask;
    }

    public async Task<List<TeamMember>> GetAllTeamMembersAsync()
    {
        return await _context.TeamMembers.AsNoTracking().ToListAsync();
    }

    public async Task<TeamMember?> GetTeamMemberByIdAsync(int id)
    {
        return await _context.TeamMembers.FindAsync(id);
    }

    public async Task AddTeamMemberAsync(TeamMember member)
    {
        await _context.TeamMembers.AddAsync(member);
    }

    public Task UpdateTeamMemberAsync(TeamMember member)
    {
        _context.TeamMembers.Update(member);
        return Task.CompletedTask;
    }

    public Task DeleteTeamMemberAsync(TeamMember member)
    {
        _context.TeamMembers.Remove(member);
        return Task.CompletedTask;
    }
}
