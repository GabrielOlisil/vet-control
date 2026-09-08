namespace Api.Models;

public class Vacina : IAuditedEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public uint ReaplicarEmXDias { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
