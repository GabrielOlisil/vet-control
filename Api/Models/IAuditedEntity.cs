namespace Api.Models;

public interface IAuditedEntity
{
    DateTimeOffset CreationDateTime { get; set; }
    DateTimeOffset? LastModificationDateTime { get; set; }
}