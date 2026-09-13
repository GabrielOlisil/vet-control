using Api.Models.Enums;

namespace Api.DTOs;

public sealed record AnimalSearchDto
{
    public Guid? RacaId { get; init; }
    public Guid? EspecieId { get; init; }
    public SexoAnimal? Sexo { get; init; }
    public OrigemAnimal? Origem { get; init; }
    public OrigemAnimal? OrigemAnimal { get; init; }
    public DateOnly? DataNascimento { get; init; }
    public DateOnly? DataNascimentoFrom { get; init; }
    public DateOnly? DataNascimentoTo { get; init; }
    public string? LoteOuPasto { get; init; }
    public bool? Ativo { get; init; }
    public string? Identificador { get; init; }
    public TipoIdentificador? TipoIdentificador { get; init; }
    public DateTimeOffset? CreationDateTimeFrom { get; init; }
    public DateTimeOffset? CreationDateTimeTo { get; init; }
}

public sealed record AplicacaoVacinaSearchDto
{
    public Guid? VacinaId { get; init; }
    public Guid? AnimalId { get; init; }
    public DateOnly? DataAplicacaoFrom { get; init; }
    public DateOnly? DataAplicacaoTo { get; init; }
    public DateOnly? DataProximaDoseFrom { get; init; }
    public DateOnly? DataProximaDoseTo { get; init; }
    public DateOnly? DataLimite { get; init; }
    public bool? SomenteAtrasadas { get; init; }
    public bool? PendenteAssinatura { get; init; }
    public bool? Proximas { get; init; }
    public bool? CicloFinalizado { get; init; }
    public StatusComprovanteVacina? StatusComprovante { get; init; }
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

public sealed record EspecieSearchDto
{
    public PorteAnimal? PortePadrao { get; init; }
}


