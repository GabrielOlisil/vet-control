using Api.Models.Enums;

namespace Api.Models;

public class MovimentacaoAnimal : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid AnimalId { get; set; }
    public DateOnly DataMovimentacao { get; set; }
    public TipoMovimentacaoAnimal Tipo { get; set; } = TipoMovimentacaoAnimal.Transferencia;
    public string? LocalOrigem { get; set; }
    public string? LocalDestino { get; set; }
    public string? Motivo { get; set; }
    public string? Observacoes { get; set; }

    public Animal? Animal { get; set; }

    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
    public Guid? CreatedByUser { get; set; }
    public Guid? UpdatedByUser { get; set; }
}
