using System.ComponentModel.DataAnnotations;
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

public sealed class AnimalPatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Name { get; set; }

    [DataType(DataType.Date)] public DateOnly? DataNascimento { get; set; }

    public string? PictureUpload { get; set; }

    [NonEmptyGuid] public Guid? RacaId { get; set; }

    public Guid? CartaoVacinaId { get; set; }
}

public sealed class AnimalReadDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid? RacaId { get; set; }

    public DateOnly DataNascimento { get; set; }

    public string? PictureUpload { get; set; }

    public Guid? CartaoVacinaId { get; set; }
}

public sealed class AnimalResponseDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public DateOnly DataNascimento { get; set; }

    public string? PictureUpload { get; set; }

    public RacaInfoDto? Raca { get; set; }

    public CartaoVacinaInfoDto? CartaoVacina { get; set; }
}

public sealed class RacaInfoDto
{
    public Guid Id { get; set; }

    public required string Nome { get; set; }

    public required string EspecieNome { get; set; }
}

public sealed class CartaoVacinaInfoDto
{
    public Guid Id { get; set; }

    public List<AplicacaoVacinaInfoDto> VacinasAplicadas { get; set; } = [];
}

public sealed class AplicacaoVacinaInfoDto
{
    public Guid Id { get; set; }

    public required string VacinaName { get; set; }

    public uint ReaplicarEmXDias { get; set; }

    public DateOnly DataAplicacao { get; set; }
}