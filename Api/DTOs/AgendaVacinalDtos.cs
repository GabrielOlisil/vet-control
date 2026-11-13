using Api.Models.Enums;

namespace Api.DTOs;

public enum OrigemPendenciaVacinal { Protocolo, Avulsa }

public sealed record AgendaVacinalSearchDto
{
    public Guid? AnimalId { get; init; }
    public Guid? VacinaId { get; init; }
    public Guid? EspecieId { get; init; }
    public Guid? ProtocoloVacinalId { get; init; }
    public string? LoteOuPasto { get; init; }
    public Guid? AmbienteId { get; init; }
    public OrigemPendenciaVacinal? Origem { get; init; }
    public DateOnly? DataPrevistaFrom { get; init; }
    public DateOnly? DataPrevistaTo { get; init; }
    public StatusPendenciaVacinal? Status { get; init; }
    public bool SomenteObrigatorias { get; init; }
    public bool IncluirConcluidas { get; init; }
}

public sealed record AgendaVacinalItemDto
{
    public OrigemPendenciaVacinal Origem { get; init; }
    public Guid AnimalId { get; init; }
    public string? AnimalNome { get; init; }
    public string? IdentificadorPrincipal { get; init; }
    public string? LoteOuPasto { get; init; }
    public Guid? AmbienteId { get; init; }
    public Guid? EspecieId { get; init; }
    public Guid VacinaId { get; init; }
    public string VacinaNome { get; init; } = "";
    public Guid? ProtocoloVacinalId { get; init; }
    public string? ProtocoloNome { get; init; }
    public int? ProtocoloVersao { get; init; }
    public Guid? AnimalProtocoloVacinalId { get; init; }
    public Guid? ProtocoloVacinalDoseId { get; init; }
    public Guid? AplicacaoVacinaId { get; init; }
    public DateOnly DataPrevista { get; init; }
    public DateOnly? DataAplicacao { get; init; }
    public int? Ordem { get; init; }
    public bool Obrigatoria { get; init; }
    public StatusPendenciaVacinal Status { get; init; }
}
