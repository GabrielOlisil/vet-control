using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.DTOs;

public sealed record VacinaCreateDto
{
    public bool Ativo { get; init; } = true;
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Descricao { get; init; }

    public bool ObrigatorioOrgaoSanitario { get; init; } = false;

    public Guid? EspecieId { get; init; }
}

public sealed record VacinaPatchDto
{
    public bool? Ativo { get; init; }
    private string? _descricao;
    private Guid? _especieId;

    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

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

    public bool? ObrigatorioOrgaoSanitario { get; init; }

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
}

public sealed record VacinaShortResponseDto
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Descricao { get; init; }
}

public sealed record VacinaReadResponseDto
{
    public bool Ativo { get; init; } = true;
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Descricao { get; init; }

    public bool ObrigatorioOrgaoSanitario { get; init; }

    public Guid? EspecieId { get; init; }

    public EspecieShortResponseDto? Especie { get; init; }
}

public sealed record VacinaDetailResponseDto
{
    public bool Ativo { get; init; } = true;
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Descricao { get; init; }

    public bool ObrigatorioOrgaoSanitario { get; init; }

    public Guid? EspecieId { get; init; }

    public EspecieReadResponseDto? Especie { get; init; }

    public uint TotalAplicacoes { get; init; }

    public DateTimeOffset CreationDateTime { get; init; }

    public DateTimeOffset? LastModificationDateTime { get; init; }

    public DateTime CriadoEm => CreationDateTime.DateTime;
}
