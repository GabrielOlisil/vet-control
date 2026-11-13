using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Api.Models;
using Api.Models.Enums;

namespace Api.DTOs;

public sealed record ProtocoloVacinalDoseCreateDto
{
    [Required]
    [NonEmptyGuid]
    public Guid VacinaId { get; init; }

    [Range(1, int.MaxValue)]
    public int Ordem { get; init; }

    [Range(0, int.MaxValue)]
    public int DiasAposInicio { get; init; }

    [Range(1, int.MaxValue)]
    public int? IntervaloReforcoDias { get; init; }

    public bool Obrigatoria { get; init; } = true;

    [StringLength(500)]
    public string? Observacoes { get; init; }
}

public sealed record ProtocoloVacinalCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Nome { get; init; }

    [StringLength(500)]
    public string? Descricao { get; init; }

    [StringLength(100)]
    public string? Finalidade { get; init; }

    [NonEmptyGuid]
    public Guid? EspecieId { get; init; }

    public bool Ativo { get; init; } = true;

    [Range(0, int.MaxValue)]
    public int? IdadeMinimaDias { get; init; }

    [Range(0, int.MaxValue)]
    public int? IdadeMaximaDias { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "O protocolo deve conter pelo menos uma dose.")]
    public required List<ProtocoloVacinalDoseCreateDto> Doses { get; init; }
}

public sealed record ProtocoloVacinalPatchDto
{
    private string? _descricao;
    private string? _finalidade;
    private Guid? _especieId;
    private List<ProtocoloVacinalDoseCreateDto>? _doses;
    private int? _idadeMinimaDias, _idadeMaximaDias;

    [StringLength(100, MinimumLength = 1)]
    public string? Nome { get; init; }

    [StringLength(500)]
    public string? Descricao
    {
        get => _descricao;
        init
        {
            _descricao = value;
            DescricaoWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool DescricaoWasProvided { get; private set; }

    [StringLength(100)]
    public string? Finalidade
    {
        get => _finalidade;
        init
        {
            _finalidade = value;
            FinalidadeWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool FinalidadeWasProvided { get; private set; }

    public Guid? EspecieId
    {
        get => _especieId;
        init
        {
            _especieId = value;
            EspecieIdWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool EspecieIdWasProvided { get; private set; }

    public bool? Ativo { get; init; }
    [Range(0, int.MaxValue)]
    public int? IdadeMinimaDias
    {
        get => _idadeMinimaDias;
        init { _idadeMinimaDias = value; IdadeMinimaDiasWasProvided = true; }
    }
    [JsonIgnore]
    public bool IdadeMinimaDiasWasProvided { get; private set; }
    [Range(0, int.MaxValue)]
    public int? IdadeMaximaDias
    {
        get => _idadeMaximaDias;
        init { _idadeMaximaDias = value; IdadeMaximaDiasWasProvided = true; }
    }
    [JsonIgnore]
    public bool IdadeMaximaDiasWasProvided { get; private set; }

    public List<ProtocoloVacinalDoseCreateDto>? Doses
    {
        get => _doses;
        init
        {
            _doses = value;
            DosesWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool DosesWasProvided { get; private set; }
}

public sealed record ProtocoloVacinalDoseResponseDto
{
    public Guid Id { get; init; }
    public Guid ProtocoloVacinalId { get; init; }
    public Guid VacinaId { get; init; }
    public int Ordem { get; init; }
    public int DiasAposInicio { get; init; }
    public int? IntervaloReforcoDias { get; init; }
    public bool Obrigatoria { get; init; }
    public string? Observacoes { get; init; }
    public VacinaShortResponseDto? Vacina { get; init; }
}

public sealed record ProtocoloVacinalShortResponseDto
{
    public Guid FamiliaId { get; init; }
    public int Versao { get; init; }
    public bool VersaoAtual { get; init; }
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? Finalidade { get; init; }
    public EspecieShortResponseDto? Especie { get; init; }
}

public sealed record ProtocoloVacinalReadResponseDto
{
    public Guid FamiliaId { get; init; }
    public int Versao { get; init; }
    public bool VersaoAtual { get; init; }
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public string? Finalidade { get; init; }
    public Guid? EspecieId { get; init; }
    public EspecieShortResponseDto? Especie { get; init; }
    public bool Ativo { get; init; }
    public int? IdadeMinimaDias { get; init; }
    public int? IdadeMaximaDias { get; init; }
    public int DoseCount { get; init; }
}

public sealed record ProtocoloVacinalDetailResponseDto
{
    public Guid FamiliaId { get; init; }
    public int Versao { get; init; }
    public bool VersaoAtual { get; init; }
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public string? Finalidade { get; init; }
    public Guid? EspecieId { get; init; }
    public EspecieShortResponseDto? Especie { get; init; }
    public bool Ativo { get; init; }
    public int? IdadeMinimaDias { get; init; }
    public int? IdadeMaximaDias { get; init; }
    public int AnimalCount { get; init; }
    public List<ProtocoloVacinalDoseResponseDto> Doses { get; init; } = [];
    public DateTimeOffset CreationDateTime { get; init; }
    public DateTimeOffset? LastModificationDateTime { get; init; }
}

public sealed record AnimalProtocoloVacinalCreateDto
{
    [Required]
    [NonEmptyGuid]
    public Guid ProtocoloVacinalId { get; init; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataInicio { get; init; }

    public bool Ativo { get; init; } = true;

    public string? Observacoes { get; init; }
}

public sealed record AnimalProtocoloVacinalPatchDto
{
    [DataType(DataType.Date)]
    public DateOnly? DataInicio { get; init; }

    public bool? Ativo { get; init; }

    private string? _observacoes;

    public string? Observacoes
    {
        get => _observacoes;
        init
        {
            _observacoes = value;
            ObservacoesWasProvided = true;
        }
    }

    [JsonIgnore]
    public bool ObservacoesWasProvided { get; private set; }
}

public sealed record MigracaoProtocoloVacinalDto
{
    [NonEmptyGuid] public Guid ProtocoloDestinoId { get; init; }
    [PastOrPresentDate] public DateOnly DataInicio { get; init; }
    [Required, StringLength(1000, MinimumLength = 3)] public required string Motivo { get; init; }
}

public sealed record AnimalProtocoloVacinalDoseResponseDto
{
    public Guid ProtocoloVacinalDoseId { get; init; }
    public int Ordem { get; init; }
    public VacinaShortResponseDto? Vacina { get; init; }
    public DateOnly DataPrevista { get; init; }
    public DateOnly? DataAplicacao { get; init; }
    public Guid? AplicacaoVacinaId { get; init; }
    public StatusPendenciaVacinal Status { get; init; }
    public bool Obrigatoria { get; init; }
}

public sealed record AnimalProtocoloVacinalReadResponseDto
{
    public Guid Id { get; init; }
    public Guid AnimalId { get; init; }
    public Guid ProtocoloVacinalId { get; init; }
    public ProtocoloVacinalShortResponseDto? ProtocoloVacinal { get; init; }
    public DateOnly DataInicio { get; init; }
    public bool Ativo { get; init; }
    public string? Observacoes { get; init; }
    public int DosesConcluidas { get; init; }
    public int DosesPendentes { get; init; }
    public int DosesAtrasadas { get; init; }
    public List<AnimalProtocoloVacinalDoseResponseDto> Doses { get; init; } = [];
    public DateTimeOffset CreationDateTime { get; init; }
    public DateTimeOffset? LastModificationDateTime { get; init; }
}

public sealed record ProtocoloVacinalPendenciaResponseDto
{
    public Guid AnimalProtocoloVacinalId { get; init; }
    public Guid AnimalId { get; init; }
    public Guid ProtocoloVacinalId { get; init; }
    public Guid ProtocoloVacinalDoseId { get; init; }
    public AnimalShortResponseDto? Animal { get; init; }
    public ProtocoloVacinalShortResponseDto? ProtocoloVacinal { get; init; }
    public VacinaShortResponseDto? Vacina { get; init; }
    public int Ordem { get; init; }
    public DateOnly DataPrevista { get; init; }
    public DateOnly? DataAplicacao { get; init; }
    public Guid? AplicacaoVacinaId { get; init; }
    public bool Obrigatoria { get; init; }
    public StatusPendenciaVacinal Status { get; init; }
}
