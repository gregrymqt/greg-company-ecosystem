using System;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.About.Domain.Entities;
using MeuCrudCsharp.Features.About.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.About.Infrastructure.Persistence.Repositories;

public class AboutRepository : IAboutRepository
{
    private readonly IMongoCollection<AboutSection> _aboutSections;
    private readonly IMongoCollection<TeamMember> _teamMembers;

    public AboutRepository(IMongoDbContext context)
    {
        _aboutSections = context.GetCollection<AboutSection>("about_sections");
        _teamMembers = context.GetCollection<TeamMember>("team_members");
    }

    public async Task<List<AboutSection>> GetAllSectionsAsync()
    {
        return await _aboutSections.Find(FilterDefinition<AboutSection>.Empty).SortBy(s => s.OrderIndex).ToListAsync();
    }

    public async Task<AboutSection?> GetSectionByIdAsync(string id)
    {
        return await _aboutSections.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddSectionAsync(AboutSection section)
    {
        await _aboutSections.InsertOneAsync(section);
    }

    public async Task UpdateSectionAsync(AboutSection section)
    {
        await _aboutSections.ReplaceOneAsync(s => s.Id == section.Id, section);
    }

    public async Task DeleteSectionAsync(AboutSection section)
    {
        await _aboutSections.DeleteOneAsync(s => s.Id == section.Id);
    }

    public async Task<List<TeamMember>> GetAllTeamMembersAsync()
    {
        return await _teamMembers.Find(FilterDefinition<TeamMember>.Empty).ToListAsync();
    }

    public async Task<TeamMember?> GetTeamMemberByIdAsync(string id)
    {
        return await _teamMembers.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddTeamMemberAsync(TeamMember member)
    {
        await _teamMembers.InsertOneAsync(member);
    }

    public async Task UpdateTeamMemberAsync(TeamMember member)
    {
        await _teamMembers.ReplaceOneAsync(m => m.Id == member.Id, member);
    }

    public async Task DeleteTeamMemberAsync(TeamMember member)
    {
        await _teamMembers.DeleteOneAsync(m => m.Id == member.Id);
    }
}

