using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuCrudCsharp.Features.Files.Domain.Entities;

namespace MeuCrudCsharp.Features.Files.Infrastructure.Persistence.Configurations;

public class EntityFileConfiguration : IEntityTypeConfiguration<EntityFile>
{
    public void Configure(EntityTypeBuilder<EntityFile> builder)
    {
        builder.ToTable("Files");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NomeArquivo)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.CaminhoRelativo)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FeatureCategoria)
            .IsRequired()
            .HasMaxLength(100);
    }
}
