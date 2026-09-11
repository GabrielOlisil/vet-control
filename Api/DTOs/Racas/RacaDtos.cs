using System.ComponentModel.DataAnnotations;
using Api.DTOs.Especies;
using Api.Models;

namespace Api.DTOs.Racas;

public sealed class RacaCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Nome { get; set; }

    [Required][NonEmptyGuid] public Guid EspecieId { get; set; }
}

public sealed class RacaPatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Nome { get; set; }
    public Guid? EspecieId { get; set; }
}

public sealed class RacaShortResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
}

public sealed class RacaReadResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public Guid EspecieId { get; set; }

    public required EspecieShortResponseDto Especie { get; set; }
}

public sealed class RacaDetailResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public Guid EspecieId { get; set; }

    public EspecieReadResponseDto? Especie { get; set; }
    public uint AnimalCount { get; set; }

    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset? LastModificationDateTime { get; set; }
}

