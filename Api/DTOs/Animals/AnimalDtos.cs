using System.ComponentModel.DataAnnotations;
using Api.DTOs.CartoesVacina;
using Api.DTOs.Racas;
using Api.Models;

namespace Api.DTOs.Animals;

public sealed class AnimalCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataNascimento { get; set; }

    public string? PictureUpload { get; set; }

    [NonEmptyGuid] public Guid? RacaId { get; set; }

    public Guid? CartaoVacinaId { get; set; }
}

public sealed class AnimalShortResponseDto()
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
}

public sealed class AnimalPatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Name { get; set; }

    [DataType(DataType.Date)] public DateOnly? DataNascimento { get; set; }

    public string? PictureUpload { get; set; }

    public Guid? RacaId { get; set; }

    public Guid? CartaoVacinaId { get; set; }
}

public sealed class AnimaReadResponseDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public RacaShortResponseDto? Raca { get; set; }

    public DateOnly DataNascimento { get; set; }
}

public sealed class AnimalDetailResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public DateOnly DataNascimento { get; set; }

    public string? PictureUpload { get; set; }

    public RacaShortResponseDto? Raca { get; set; }

    public CartaoVacinaShortResponseDto? CartaoVacina { get; set; } //Cartao de vacina short
}


