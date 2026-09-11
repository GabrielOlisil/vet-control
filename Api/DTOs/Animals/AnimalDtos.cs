using System.ComponentModel.DataAnnotations;
using Api.DTOs.Racas;
using Api.Models;
using Api.Models.Enums;
using Api.DTOs.AplicacoesVacina;

namespace Api.DTOs.Animals;

public record IdentificadorCreateDto(TipoIdentificador Tipo, string Valor, bool EhPrincipal = false);
public record IdentificadorResponseDto(Guid Id, TipoIdentificador Tipo, string Valor, bool EhPrincipal);

public sealed class AnimalCreateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataNascimento { get; set; }

    public bool DataNascimentoAproximada { get; set; }

    [NonEmptyGuid] public Guid? RacaId { get; set; }

    public SexoAnimal Sexo { get; set; } = SexoAnimal.Indefinido;

    public OrigemAnimal Origem { get; set; } = OrigemAnimal.Externo;

    public string? LoteOuPasto { get; set; }

    public bool Ativo { get; set; } = true;

    [Required]
    [MinLength(1)]
    public required List<IdentificadorCreateDto> Identificadores { get; set; }
}

public sealed class AnimalPatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Name { get; set; }

    [DataType(DataType.Date)] public DateOnly? DataNascimento { get; set; }

    public bool? DataNascimentoAproximada { get; set; }

    public Guid? RacaId { get; set; }

    public SexoAnimal? Sexo { get; set; }

    public OrigemAnimal? Origem { get; set; }

    public string? LoteOuPasto { get; set; }

    public bool? Ativo { get; set; }

    public List<IdentificadorCreateDto>? Identificadores { get; set; }
}

public sealed class AnimalShortResponseDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? IdentificadorPrincipal { get; set; }
}

public class AnimalReadResponseDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public SexoAnimal Sexo { get; set; }

    public OrigemAnimal Origem { get; set; }

    public DateOnly DataNascimento { get; set; }

    public string? LoteOuPasto { get; set; }

    public bool Ativo { get; set; } = true;

    public RacaShortResponseDto? Raca { get; set; }

    public IdentificadorResponseDto? IdentificadorPrincipal { get; set; }
}

// Alias de compatibilidade com versões anteriores
public sealed class AnimaReadResponseDto : AnimalReadResponseDto
{
}

public sealed class AnimalDetailResponseDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public DateOnly DataNascimento { get; set; }

    public bool DataNascimentoAproximada { get; set; }

    public SexoAnimal Sexo { get; set; }

    public OrigemAnimal Origem { get; set; }

    public string? LoteOuPasto { get; set; }

    public bool Ativo { get; set; } = true;

    public RacaReadResponseDto? Raca { get; set; }

    public List<IdentificadorResponseDto> Identificadores { get; set; } = [];

    public IdentificadorResponseDto? IdentificadorPrincipal { get; set; }

    public DateTimeOffset CreationDateTime { get; set; }

    public DateTimeOffset? LastModificationDateTime { get; set; }
}

public sealed class AnimalProntuarioResponseDto
{
    public required AnimalDetailResponseDto Animal { get; set; }
    public List<AplicacaoVacinaDetailResponseDto> AplicacoesVacina { get; set; } = [];
}

