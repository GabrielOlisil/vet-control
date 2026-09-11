using System.ComponentModel.DataAnnotations;
using Api.Models;
using Api.Models.Enums;

namespace Api.DTOs;

public sealed record IdentificadorCreateDto(TipoIdentificador Tipo, string Valor, bool EhPrincipal = false);
public sealed record IdentificadorResponseDto(Guid Id, TipoIdentificador Tipo, string Valor, bool EhPrincipal);

public sealed record AnimalCreateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataNascimento { get; init; }

    public bool DataNascimentoAproximada { get; init; }

    [NonEmptyGuid] public Guid? RacaId { get; init; }

    public SexoAnimal Sexo { get; init; } = SexoAnimal.Indefinido;

    public OrigemAnimal Origem { get; init; } = OrigemAnimal.Externo;

    public string? LoteOuPasto { get; init; }

    public bool Ativo { get; init; } = true;

    [Required]
    [MinLength(1)]
    public required List<IdentificadorCreateDto> Identificadores { get; init; }
}

public sealed record AnimalPatchDto
{
    [StringLength(100, MinimumLength = 1)] public string? Name { get; init; }

    [DataType(DataType.Date)] public DateOnly? DataNascimento { get; init; }

    public bool? DataNascimentoAproximada { get; init; }

    public Guid? RacaId { get; init; }

    public SexoAnimal? Sexo { get; init; }

    public OrigemAnimal? Origem { get; init; }

    public string? LoteOuPasto { get; init; }

    public bool? Ativo { get; init; }

    public List<IdentificadorCreateDto>? Identificadores { get; init; }
}

public sealed record AnimalShortResponseDto
{
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public string? IdentificadorPrincipal { get; init; }
}

public sealed record AnimalReadResponseDto
{
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public SexoAnimal Sexo { get; init; }

    public OrigemAnimal Origem { get; init; }

    public DateOnly DataNascimento { get; init; }

    public string? LoteOuPasto { get; init; }

    public bool Ativo { get; init; } = true;

    public RacaShortResponseDto? Raca { get; init; }

    public IdentificadorResponseDto? IdentificadorPrincipal { get; init; }
}

public sealed record AnimalDetailResponseDto
{
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public DateOnly DataNascimento { get; init; }

    public bool DataNascimentoAproximada { get; init; }

    public SexoAnimal Sexo { get; init; }

    public OrigemAnimal Origem { get; init; }

    public string? LoteOuPasto { get; init; }

    public bool Ativo { get; init; } = true;

    public RacaReadResponseDto? Raca { get; init; }

    public List<IdentificadorResponseDto> Identificadores { get; init; } = [];

    public IdentificadorResponseDto? IdentificadorPrincipal { get; init; }

    public DateTimeOffset CreationDateTime { get; init; }

    public DateTimeOffset? LastModificationDateTime { get; init; }
}

public sealed class AnimalProntuarioResponseDto
{
    public required AnimalDetailResponseDto Animal { get; init; }
    public List<AplicacaoVacinaDetailResponseDto> AplicacoesVacina { get; init; } = [];
}

