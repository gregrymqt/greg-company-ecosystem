using MeuCrudCsharp.Models;
using MeuCrudCsharp.Features.About.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using MeuCrudCsharp.Features.Home.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Data
{
    public class ApiDbContext : IdentityDbContext<Users, Roles, string>
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options) { }

        public DbSet<Payments> Payments { get; set; } = null!;
        public DbSet<Video> Videos { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Plan> Plans { get; set; } = null!;
        public DbSet<Claims> Claims { get; set; } = null!;
        public DbSet<Chargeback> Chargebacks { get; set; } = null!;
        public DbSet<EntityFile> Files { get; set; } = null!;
        public DbSet<HomeHero> HomeHeroes { get; set; } = null!;
        public DbSet<HomeServiceEntry> HomeServices { get; set; } = null!;
        public DbSet<AboutSection> AboutSections { get; set; } = null!;
        public DbSet<TeamMember> TeamMembers { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);

            modelBuilder
                .Entity<Payments>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);

            modelBuilder
                .Entity<Payments>()
                .HasOne(p => p.Subscription)
                .WithMany()
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Plan>().Property(p => p.FrequencyType).HasConversion<string>();
        }
    }
}
