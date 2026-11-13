using Api.Models.Enums;

namespace Api.Models;

public sealed class Ambiente : IAuditedEntity
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string NomeNormalizado { get; private set; } = null!;
    public TipoAmbiente Tipo { get; set; }
    public List<Animal> Animais { get; set; } = [];
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
    public Guid? CreatedByUser { get; set; }
    public Guid? UpdatedByUser { get; set; }
}
