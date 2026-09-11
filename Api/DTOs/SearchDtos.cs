using Api.Models.Enums;

namespace Api.DTOs;

public sealed record AnimalSearchDto
{
    public Guid? RacaId { get; init; }
    public SexoAnimal? Sexo { get; init; }
    public OrigemAnimal? Origem { get; init; }
    public bool? Ativo { get; init; }
    public string? LoteOuPasto { get; init; }
    public DateOnly? DataNascimentoFrom { get; init; }
    public DateOnly? DataNascimentoTo { get; init; }
}

public sealed record AplicacaoVacinaSearchDto
{
    public Guid? VacinaId { get; init; }
    public Guid? AnimalId { get; init; }
    public DateOnly? DataAplicacaoFrom { get; init; }
    public DateOnly? DataAplicacaoTo { get; init; }
}

public sealed record RacaSearchDto
{
    public Guid? EspecieId { get; init; }
}

public sealed record VacinaSearchDto
{
    public Guid? EspecieId { get; init; }
    public bool? ObrigatorioOrgaoSanitario { get; init; }
    public uint? ReaplicarEmXDiasMin { get; init; }
    public uint? ReaplicarEmXDiasMax { get; init; }
}

