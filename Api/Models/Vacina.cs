namespace Api.Models;

public class Vacina : IAuditedEntity
{
    public Guid Id { get; set; }

    public Guid? EspecieId { get; set; }
    public required string Name { get; set; }

    public string? Descricao { get; set; }

    public uint ReaplicarEmXDias { get; set; }

    public bool ObrigatorioOrgaoSanitario { get; set; } = false;




    public List<AplicacaoVacina> Aplicacoes { get; set; } = [];

    public Especie? Especie { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}
