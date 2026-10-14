using Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public class ApiContext(DbContextOptions<ApiContext> options, IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    public DbSet<Especie> Especies { get; set; }
    public DbSet<Raca> Racas { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<IdentificadorAnimal> IdentificadorAnimal { get; set; }
    public DbSet<Vacina> Vacinas { get; set; }
    public DbSet<AplicacaoVacina> AplicacoesVacina { get; set; }
    public DbSet<ProtocoloVacinal> ProtocolosVacinais { get; set; }
    public DbSet<ProtocoloVacinalDose> ProtocoloVacinalDoses { get; set; }
    public DbSet<AnimalProtocoloVacinal> AnimalProtocolosVacinais { get; set; }
    public DbSet<MovimentacaoAnimal> MovimentacoesAnimais { get; set; }
    public DbSet<AssinaturaAplicacaoVacina> AssinaturasAplicacoesVacina { get; set; }



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
        var now = DateTimeOffset.UtcNow;
        var currentUserId = GetCurrentUserId();

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
            entity.CreationDateTime = now;
            entity.CreatedByUser = currentUserId;
            entity.UpdatedByUser = currentUserId;
        }

        foreach (var entity in entitiesModified)
        {
            entity.LastModificationDateTime = now;
            if (currentUserId.HasValue)
            {
                entity.UpdatedByUser = currentUserId;
            }
        }
    }

    private Guid? GetCurrentUserId()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var subject = principal.FindFirst("sub")?.Value ??
                      principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(subject, out var userId) ? userId : null;
    }
}
