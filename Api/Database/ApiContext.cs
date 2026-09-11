using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public class ApiContext(DbContextOptions<ApiContext> options) : DbContext(options)
{
    public DbSet<Especie> Especies { get; set; }
    public DbSet<Raca> Racas { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<IdentificadorAnimal> IdentificadorAnimal { get; set; }
    public DbSet<Vacina> Vacinas { get; set; }
    public DbSet<AplicacaoVacina> AplicacoesVacina { get; set; }



    public override int SaveChanges()
    {
        SetAuditedColumns();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditedColumns();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiContext).Assembly);
    }

    private void SetAuditedColumns()
    {
        var entitiesCreated = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditedEntity, State: EntityState.Added })
            .Select(x => (IAuditedEntity)x.Entity);

        var entitiesModified = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditedEntity, State: EntityState.Modified })
            .Select(x => (IAuditedEntity)x.Entity);


        foreach (var entity in entitiesCreated)
        {
            entity.CreationDateTime = DateTimeOffset.UtcNow;
        }

        foreach (var entity in entitiesModified)
        {
            entity.LastModificationDateTime = DateTimeOffset.UtcNow;
        }
    }
}
