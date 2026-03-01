using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Data
{
    public class ApiDbContext : IdentityDbContext<Users, Roles, string>
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options) { }

        public DbSet<Payments> Payments { get; set; }

        public DbSet<Video> Videos { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Plan> Plans { get; set; }

        public DbSet<Claims> Claims { get; set; }

        public DbSet<Chargeback> Chargebacks { get; set; }

        public DbSet<EntityFile> Files { get; set; }

        public DbSet<HomeHero> HomeHeroes { get; set; }
        public DbSet<HomeService> HomeServices { get; set; }
        public DbSet<AboutSection> AboutSections { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
