namespace Api.Models;

public class ProtocoloVacinalDose : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid ProtocoloVacinalId { get; set; }
    public Guid VacinaId { get; set; }

    public int Ordem { get; set; }
    public int DiasAposInicio { get; set; }
    public int? IntervaloReforcoDias { get; set; }
    public bool Obrigatoria { get; set; } = true;
    public string? Observacoes { get; set; }

    public ProtocoloVacinal? ProtocoloVacinal { get; set; }
    public Vacina? Vacina { get; set; }
    public List<AplicacaoVacina> Aplicacoes { get; set; } = [];

    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
    public Guid? CreatedByUser { get; set; }
    public Guid? UpdatedByUser { get; set; }
}
