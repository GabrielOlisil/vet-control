using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Vacinas;

public sealed class VacinaCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [Range(1, uint.MaxValue)]
    public uint ReaplicarEmXDias { get; set; }
}

public sealed class VacinaPatchDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }

    [Range(1, uint.MaxValue)]
    public uint? ReaplicarEmXDias { get; set; }
}


public sealed class VacinaShortResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }
}

public sealed class VacinaReadResponseDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public uint ReaplicarEmXDias { get; set; }
}

public sealed class VacinaDetailResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public uint ReaplicarEmXDias { get; set; }

    public DateTime CriadoEm { get; set; }
}