using Api.Models.Enums;

namespace Api.Models;

public class IdentificadorAnimal : IAuditedEntity
{
    public Guid Id { get; set; }
    public Guid AnimalId { get; set; }
    public Animal? Animal { get; set; }

    public TipoIdentificador Tipo { get; set; } = TipoIdentificador.BrincoVisual;

    public required string Valor { get; set; }

    public bool IsPrincipal { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
