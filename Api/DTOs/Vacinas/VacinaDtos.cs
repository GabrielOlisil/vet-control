using System.ComponentModel.DataAnnotations;
using Api.DTOs.Especies;

namespace Api.DTOs.Vacinas;

public sealed class VacinaCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Range(0, uint.MaxValue)]
    public uint ReaplicarEmXDias { get; set; }

    public bool ObrigatorioOrgaoSanitario { get; set; } = false;

    public Guid? EspecieId { get; set; }
}

public sealed class VacinaPatchDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Range(0, uint.MaxValue)]
    public uint? ReaplicarEmXDias { get; set; }

    public bool? ObrigatorioOrgaoSanitario { get; set; }

    public Guid? EspecieId { get; set; }
}

public sealed class VacinaShortResponseDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Descricao { get; set; }
}

public sealed class VacinaReadResponseDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Descricao { get; set; }

    public uint ReaplicarEmXDias { get; set; }

    public bool ObrigatorioOrgaoSanitario { get; set; }

    public Guid? EspecieId { get; set; }

    public EspecieShortResponseDto? Especie { get; set; }
}

public sealed class VacinaDetailResponseDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Descricao { get; set; }

    public uint ReaplicarEmXDias { get; set; }

    public bool ObrigatorioOrgaoSanitario { get; set; }

    public Guid? EspecieId { get; set; }

    public EspecieReadResponseDto? Especie { get; set; }

    public uint TotalAplicacoes { get; set; }

    public DateTimeOffset CreationDateTime { get; set; }

    public DateTimeOffset? LastModificationDateTime { get; set; }

    public DateTime CriadoEm => CreationDateTime.DateTime;
}