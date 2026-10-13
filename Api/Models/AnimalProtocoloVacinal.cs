namespace Api.Models;

public class AnimalProtocoloVacinal : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid AnimalId { get; set; }
    public Guid ProtocoloVacinalId { get; set; }

    public DateOnly DataInicio { get; set; }
    public bool Ativo { get; set; } = true;
    public string? Observacoes { get; set; }

    public Animal? Animal { get; set; }
    public ProtocoloVacinal? ProtocoloVacinal { get; set; }
    public List<AplicacaoVacina> Aplicacoes { get; set; } = [];

    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
