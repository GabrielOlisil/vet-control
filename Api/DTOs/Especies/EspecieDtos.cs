using System.ComponentModel.DataAnnotations;
using Api.DTOs.Racas;
using Api.DTOs.Vacinas;
using Api.Models.Enums;

namespace Api.DTOs.Especies;

public sealed class EspecieCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Nome { get; set; }

    [StringLength(100, MinimumLength = 1)]
    public string? NomeCientifico { get; set; }

    public PorteAnimal PortePadrao { get; set; } = PorteAnimal.Grande;

    [StringLength(100)]
    public string IconeKey { get; set; } = "paw";
}

public sealed class EspeciePatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Nome { get; set; }
    [StringLength(100, MinimumLength = 1)] public string? NomeCientifico { get; set; }
    public PorteAnimal? PortePadrao { get; set; }
    [StringLength(100)] public string? IconeKey { get; set; }
}

public sealed class EspecieShortResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public string FullName { get; set; } = string.Empty;
}

public sealed class EspecieReadResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public string? NomeCientifico { get; set; }
    public PorteAnimal PortePadrao { get; set; }
    public string IconeKey { get; set; } = "paw";
    public uint RaceCount { get; set; }
}

public sealed class EspecieDetailResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public string? NomeCientifico { get; set; }
    public PorteAnimal PortePadrao { get; set; }
    public string IconeKey { get; set; } = "paw";
    public uint RaceCount { get; set; }

    public List<RacaReadResponseDto> Racas { get; set; } = [];
    public List<VacinaReadResponseDto> VacinasRestritas { get; set; } = [];

    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}

