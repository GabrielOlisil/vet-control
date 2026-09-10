using Api.Models.Enums;

namespace Api.Models;

public class Especie : IAuditedEntity
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public string? NomeCientifico { get; set; }

    public PorteAnimal PortePadrao { get; set; } = PorteAnimal.Grande;


    public List<Raca> Racas { get; set; } = [];
    public List<Vacina> VacinasRestritas { get; set; } = [];
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
