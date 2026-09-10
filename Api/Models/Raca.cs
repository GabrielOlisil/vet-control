namespace Api.Models;

public class Raca : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid EspecieId { get; set; }

    public required string Nome { get; set; }


    public Especie? Especie { get; set; }


    public List<Animal> Animais { get; set; } = [];
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
