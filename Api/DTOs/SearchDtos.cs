using Api.Models.Enums;

namespace Api.DTOs;

public sealed class AnimalSearchDto
{
    public Guid? RacaId { get; set; }
    public SexoAnimal? Sexo { get; set; }
    public OrigemAnimal? Origem { get; set; }
    public bool? Ativo { get; set; }
    public string? LoteOuPasto { get; set; }
    public DateOnly? DataNascimentoFrom { get; set; }
    public DateOnly? DataNascimentoTo { get; set; }
}

public sealed class AplicacaoVacinaSearchDto
{
    public Guid? VacinaId { get; set; }
    public Guid? AnimalId { get; set; }
    public DateOnly? DataAplicacaoFrom { get; set; }
    public DateOnly? DataAplicacaoTo { get; set; }
}

public sealed class RacaSearchDto
{
    public Guid? EspecieId { get; set; }
}

public sealed class VacinaSearchDto
{
    public Guid? EspecieId { get; set; }
    public bool? ObrigatorioOrgaoSanitario { get; set; }
    public uint? ReaplicarEmXDiasMin { get; set; }
    public uint? ReaplicarEmXDiasMax { get; set; }
}

