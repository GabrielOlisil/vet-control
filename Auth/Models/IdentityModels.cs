using Microsoft.AspNetCore.Identity;

namespace Auth.Models;

// Same contract used by Api.Models.IAuditedEntity, without coupling the applications.
public interface IAuditedEntity
{
    DateTimeOffset CreationDateTime { get; set; }
    DateTimeOffset? LastModificationDateTime { get; set; }
}

public sealed class ApplicationUser : IdentityUser<Guid>, IAuditedEntity
{
    public string FullName { get; set; } = "";
    public string? Crmv { get; set; }
    public string? CrmvUf { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public string SessionVersion { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>, IAuditedEntity
{
    public string Description { get; set; } = "";
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}

public sealed class UserAuditEvent : IAuditedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ActorId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = "";
    // Snapshots exclude password hashes, stamps, tokens and secrets.
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
