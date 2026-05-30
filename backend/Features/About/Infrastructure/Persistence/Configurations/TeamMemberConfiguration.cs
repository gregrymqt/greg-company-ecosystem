using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuCrudCsharp.Features.About.Domain.Entities;

namespace MeuCrudCsharp.Features.About.Infrastructure.Persistence.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("AboutTeamMembers");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Role)
            .HasMaxLength(100);

        builder.Property(x => x.LinkedinUrl)
            .HasMaxLength(200);

        builder.Property(x => x.GithubUrl)
            .HasMaxLength(200);
    }
}
