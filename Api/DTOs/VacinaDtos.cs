using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public sealed record VacinaCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Descricao { get; init; }

    [Range(0, uint.MaxValue)]
    public uint ReaplicarEmXDias { get; init; }

    public bool ObrigatorioOrgaoSanitario { get; init; } = false;

    public Guid? EspecieId { get; init; }
}

public sealed record VacinaPatchDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    [StringLength(500)]
    public string? Descricao { get; init; }

    [Range(0, uint.MaxValue)]
    public uint? ReaplicarEmXDias { get; init; }

    public bool? ObrigatorioOrgaoSanitario { get; init; }

    public Guid? EspecieId { get; init; }
}

public sealed record VacinaShortResponseDto
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Descricao { get; init; }
}

public sealed record VacinaReadResponseDto
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Descricao { get; init; }

    public int ReaplicarEmXDias { get; init; }

    public bool ObrigatorioOrgaoSanitario { get; init; }

    public Guid? EspecieId { get; init; }

    public EspecieShortResponseDto? Especie { get; init; }
}

public sealed record VacinaDetailResponseDto
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Descricao { get; init; }

    public uint ReaplicarEmXDias { get; init; }

    public bool ObrigatorioOrgaoSanitario { get; init; }

    public Guid? EspecieId { get; init; }

    public EspecieReadResponseDto? Especie { get; init; }

    public uint TotalAplicacoes { get; init; }

    public DateTimeOffset CreationDateTime { get; init; }

    public DateTimeOffset? LastModificationDateTime { get; init; }

    public DateTime CriadoEm => CreationDateTime.DateTime;
}