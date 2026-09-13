using System.ComponentModel.DataAnnotations;

using Api.Models.Enums;

namespace Api.DTOs;

public sealed record EspecieCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Nome { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? NomeCientifico { get; init; }

    public PorteAnimal PortePadrao { get; init; } = PorteAnimal.Grande;

    [StringLength(100)]
    public string IconeKey { get; init; } = "paw";
}

public sealed record EspeciePatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Nome { get; init; }
    [StringLength(100, MinimumLength = 1)] public string? NomeCientifico { get; init; }
    public PorteAnimal? PortePadrao { get; init; }
    [StringLength(100)] public string? IconeKey { get; init; }
}

public sealed record EspecieShortResponseDto
{
    public Guid Id { get; init; }

    public required string Nome { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string IconeKey { get; set; } = string.Empty;
}

public sealed record EspecieReadResponseDto
{
    public Guid Id { get; init; }

    public required string Nome { get; init; }
    public string? NomeCientifico { get; init; }
    public PorteAnimal PortePadrao { get; init; }
    public string IconeKey { get; init; } = "paw";
    public uint RaceCount { get; init; }
}

public sealed record EspecieDetailResponseDto
{
    public Guid Id { get; init; }

    public required string Nome { get; init; }
    public string? NomeCientifico { get; init; }
    public PorteAnimal PortePadrao { get; init; }
    public string IconeKey { get; init; } = "paw";
    public uint RaceCount { get; init; }

    public List<RacaReadResponseDto> Racas { get; init; } = [];
    public List<VacinaReadResponseDto> VacinasRestritas { get; init; } = [];

    public DateTimeOffset CreationDateTime { get; init; }
    public DateTimeOffset? LastModificationDateTime { get; init; }
}

