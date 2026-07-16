using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Domain.Entities;
using MeuCrudCsharp.Features.Home.Domain.Entities;
using MeuCrudCsharp.Features.Files.Domain.Entities;
using MeuCrudCsharp.Features.Support.Domain.Entities;
using MeuCrudCsharp.Features.Products.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;

namespace MeuCrudCsharp.Data;

public class ApplicationDbContext : IdentityDbContext<Users, Roles, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<HomeHero> HomeHeroes => Set<HomeHero>();
    public DbSet<HomeServiceEntry> HomeServices => Set<HomeServiceEntry>();
    public DbSet<EntityFile> EntityFiles => Set<EntityFile>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Chargeback> Chargebacks => Set<Chargeback>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Renomeia tabelas do Identity para padrão
        modelBuilder.Entity<Users>().ToTable("Users");
        modelBuilder.Entity<Roles>().ToTable("Roles");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
