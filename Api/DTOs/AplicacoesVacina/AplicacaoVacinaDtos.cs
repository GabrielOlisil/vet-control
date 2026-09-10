using System.ComponentModel.DataAnnotations;
using Api.DTOs.Vacinas;
using Api.Models;
using Api.Models.Enums;

namespace Api.DTOs.AplicacoesVacina;

public sealed class AplicacaoVacinaCreateDto
{
    [Required]
    [NonEmptyGuid]
    public Guid AnimalId { get; set; }

    [Required]
    [NonEmptyGuid]
    public Guid VacinaId { get; set; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataAplicacao { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; set; }

    [Required]
    public required string NumeroLote { get; set; }

    public string? LaboratorioFabricante { get; set; }

    public decimal? DoseMl { get; set; }

    [Required]
    public required string VeterinarioResponsavel { get; set; }

    public string? Aplicador { get; set; }

    public string? Observacoes { get; set; }
}

public sealed class AplicacaoVacinaPatchDto
{
    [DataType(DataType.Date)]
    public DateOnly? DataAplicacao { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DataProximaDose { get; set; }

    public string? NumeroLote { get; set; }

    public string? LaboratorioFabricante { get; set; }

    public decimal? DoseMl { get; set; }

    public string? VeterinarioResponsavel { get; set; }

    public string? Aplicador { get; set; }

    public string? Observacoes { get; set; }
}

public sealed class AplicacaoVacinaReadResponseDto
{
    public Guid Id { get; set; }

    public required VacinaShortResponseDto Vacina { get; set; }

    public Guid AnimalId { get; set; }

    public DateOnly DataAplicacao { get; set; }
}

public sealed class AplicacaoVacinaShortResponseDto
{
    public Guid Id { get; set; }
    public required VacinaShortResponseDto Vacina { get; set; }
    public DateOnly DataAplicacao { get; set; }
}

public sealed class AplicacaoVacinaDetailResponseDto
{
    public Guid Id { get; set; }

    public Guid AnimalId { get; set; }

    public Guid VacinaId { get; set; }

    public VacinaDetailResponseDto? Vacina { get; set; }

    public DateOnly DataAplicacao { get; set; }

    public DateOnly DataProximaDose { get; set; }

    public required string NumeroLote { get; set; }

    public string? LaboratorioFabricante { get; set; }

    public decimal? DoseMl { get; set; }

    public required string VeterinarioResponsavel { get; set; }

    public string? Aplicador { get; set; }

    public string? Observacoes { get; set; }

    public StatusComprovanteVacina StatusComprovante { get; set; }

    public bool TemComprovanteAnexo { get; set; }

    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset? AtualizadoEm { get; set; }
}
