using System.ComponentModel.DataAnnotations;
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

public sealed class EspecieReadResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public string? NomeCientifico { get; set; }
    public PorteAnimal PortePadrao { get; set; }
    public string IconeKey { get; set; } = "paw";
}

public sealed class EspecieShortResponseDto
{
    public Guid Id { get; set; }

    public required string FullName { get; set; }
}

public sealed class EspecieDetailResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public string? NomeCientifico { get; set; }
    public PorteAnimal PortePadrao { get; set; }
    public string IconeKey { get; set; } = "paw";
    public uint RaceCount { get; set; }
}
