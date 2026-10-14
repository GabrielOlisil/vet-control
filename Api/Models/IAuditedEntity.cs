namespace Api.Models;

public interface IAuditedEntity
{
    DateTimeOffset CreationDateTime { get; set; }
    DateTimeOffset? LastModificationDateTime { get; set; }
    Guid? CreatedByUser { get; set; }
    Guid? UpdatedByUser { get; set; }
}
