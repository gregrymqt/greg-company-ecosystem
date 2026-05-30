using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuCrudCsharp.Features.Videos.Domain.Entities;

namespace MeuCrudCsharp.Features.Videos.Infrastructure.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CourseId);
        builder.HasIndex(x => x.PublicId).IsUnique();

        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.StorageIdentifier).IsRequired();

        builder.Property(x => x.ThumbnailUrl).HasMaxLength(2048);

        builder.HasOne(x => x.Course)
            .WithMany(c => c.Videos)
            .HasForeignKey(x => x.CourseId);

        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId);
    }
}
