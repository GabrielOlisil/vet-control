using Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth.Database;

public sealed class AuthContext(DbContextOptions<AuthContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public const string ConnectionString = "Server=localhost;Database=AppUsers;Username=postgres;Password=postgresssss;Port=5432";
    public DbSet<UserAuditEvent> AuditEvents => Set<UserAuditEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasPostgresExtension("pg_trgm");
        builder.Entity<ApplicationUser>().HasIndex(x => x.FullName).HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.Entity<ApplicationUser>().HasIndex(x => x.UserName).HasDatabaseName("IX_AspNetUsers_UserNameSearch").HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.Entity<ApplicationUser>().HasIndex(x => x.Email).HasDatabaseName("IX_AspNetUsers_EmailSearch").HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.Entity<ApplicationUser>().Property(x => x.FullName).HasMaxLength(200);
        builder.Entity<ApplicationUser>().Property(x => x.SessionVersion).HasMaxLength(32);
        builder.Entity<ApplicationRole>().Property(x => x.Description).HasMaxLength(500);
        builder.Entity<UserAuditEvent>(entity =>
        {
            entity.Property(x => x.Action).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.BeforeJson).HasColumnType("jsonb");
            entity.Property(x => x.AfterJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.UserId, x.CreationDateTime });
        });
        builder.UseOpenIddict();
    }

    private void Audit()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreationDateTime = now;
            if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreationDateTime).IsModified = false;
                entry.Entity.LastModificationDateTime = now;
            }
        }
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess) { Audit(); return base.SaveChanges(acceptAllChangesOnSuccess); }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    { Audit(); return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken); }
}

public sealed class AuthContextFactory : IDesignTimeDbContextFactory<AuthContext>
{
    public AuthContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AuthContext>()
        .UseNpgsql(AuthContext.ConnectionString).UseOpenIddict().Options);
}
