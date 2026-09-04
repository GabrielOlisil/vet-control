using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Especies;

public sealed class EspecieCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Nome { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string NomeCientifico { get; set; }
}

public sealed class EspeciePatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Nome { get; set; }
    [StringLength(100, MinimumLength = 1)] public string? NomeCientifico { get; set; }
}

public sealed class EspecieReadDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public required string NomeCientifico { get; set; }
}

public sealed class EspecieResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
    public required string NomeCientifico { get; set; }

    public List<RacaResumoDto> Racas { get; set; } = [];
}

public sealed class RacaResumoDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
}
