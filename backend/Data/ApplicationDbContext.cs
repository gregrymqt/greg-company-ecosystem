using Microsoft.EntityFrameworkCore;
using MeuCrudCsharp.Data.Interfaces;

namespace MeuCrudCsharp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplica automaticamente todas as configurações de entidades (IEntityTypeConfiguration)
        // que criaremos futuramente nas pastas de Features.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
