using System.ComponentModel.DataAnnotations;
using Api.DTOs.Especies;
using Api.Models;

namespace Api.DTOs.Racas;

public sealed class RacaCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Nome { get; set; }

    [Required] [NonEmptyGuid] public Guid EspecieId { get; set; }
}

public sealed class RacaPatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Nome { get; set; }
}

public sealed class RacaReadDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public Guid EspecieId { get; set; }
}

public sealed class RacaResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public EspecieReadDto Especie { get; set; }

    public List<AnimalResumoDto> Animais { get; set; } = [];
}

public sealed class AnimalResumoDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public DateOnly DataNascimento { get; set; }
}
