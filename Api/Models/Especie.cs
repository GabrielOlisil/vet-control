namespace Api.Models;

public class Especie : IAuditedEntity
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public required string NomeCientifico { get; set; }

    public List<Raca> Racas { get; set; } = [];
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
