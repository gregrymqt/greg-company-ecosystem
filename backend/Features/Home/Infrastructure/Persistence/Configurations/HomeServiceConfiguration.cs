using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuCrudCsharp.Features.Home.Domain.Entities;

namespace MeuCrudCsharp.Features.Home.Infrastructure.Persistence.Configurations;

public class HomeServiceConfiguration : IEntityTypeConfiguration<HomeServiceEntry>
{
    public void Configure(EntityTypeBuilder<HomeServiceEntry> builder)
    {
        builder.ToTable("HomeServices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IconClass)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}
