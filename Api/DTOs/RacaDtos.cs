using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs;

public sealed record RacaCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Nome { get; init; }

    [Required][NonEmptyGuid] public Guid EspecieId { get; init; }
}

public sealed record RacaPatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Nome { get; init; }
    public Guid? EspecieId { get; init; }
}

public sealed record RacaShortResponseDto
{
    public Guid Id { get; init; }

    public required string Nome { get; init; }
}

public sealed record RacaReadResponseDto
{
    public Guid Id { get; init; }

    public required string Nome { get; init; }

    public Guid EspecieId { get; init; }

    public required EspecieShortResponseDto Especie { get; init; }
}

public sealed record RacaDetailResponseDto
{
    public Guid Id { get; init; }

    public required string Nome { get; init; }

    public Guid EspecieId { get; init; }

    public EspecieReadResponseDto? Especie { get; init; }
    public uint AnimalCount { get; init; }

    public DateTimeOffset CreationDateTime { get; init; }
    public DateTimeOffset? LastModificationDateTime { get; init; }
}

