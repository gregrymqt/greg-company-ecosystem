using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuCrudCsharp.Features.Home.Domain.Entities;

namespace MeuCrudCsharp.Features.Home.Infrastructure.Persistence.Configurations;

public class HomeHeroConfiguration : IEntityTypeConfiguration<HomeHero>
{
    public void Configure(EntityTypeBuilder<HomeHero> builder)
    {
        builder.ToTable("HomeHeroes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Subtitle)
            .HasMaxLength(200);

        builder.Property(x => x.ActionText)
            .HasMaxLength(50);

        builder.Property(x => x.ActionUrl)
            .HasMaxLength(500);
    }
}
