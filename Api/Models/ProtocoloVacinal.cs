namespace Api.Models;

public class ProtocoloVacinal : IAuditedEntity
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; } = Guid.NewGuid();
    public int Versao { get; set; } = 1;
    public bool VersaoAtual { get; set; } = true;
    public uint Version { get; set; }

    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public string? Finalidade { get; set; }

    public Guid? EspecieId { get; set; }
    public bool Ativo { get; set; } = true;
    public int? IdadeMinimaDias { get; set; }
    public int? IdadeMaximaDias { get; set; }

    public Especie? Especie { get; set; }
    public List<ProtocoloVacinalDose> Doses { get; set; } = [];
    public List<AnimalProtocoloVacinal> AplicacoesEmAnimais { get; set; } = [];

    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
    public Guid? CreatedByUser { get; set; }
    public Guid? UpdatedByUser { get; set; }
}
