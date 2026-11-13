using System.ComponentModel.DataAnnotations;
using Api.Models;
using Api.Models.Enums;

namespace Api.DTOs;

public sealed record MovimentacaoAnimalCreateDto
{
    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataMovimentacao { get; init; }

    public TipoMovimentacaoAnimal Tipo { get; init; } = TipoMovimentacaoAnimal.Transferencia;

    [StringLength(150)]
    public string? LocalOrigem { get; init; }

    [StringLength(150)]
    public string? LocalDestino { get; init; }
    public Guid? AmbienteOrigemId { get; init; }
    public Guid? AmbienteDestinoId { get; init; }

    [StringLength(200)]
    public string? Motivo { get; init; }

    [StringLength(1000)]
    public string? Observacoes { get; init; }
}

public sealed record MovimentacaoAnimalLoteCreateDto
{
    [MinLength(1)]
    public List<Guid> AnimalIds { get; init; } = [];

    [PastOrPresentDate]
    [DataType(DataType.Date)]
    public DateOnly DataMovimentacao { get; init; }

    public TipoMovimentacaoAnimal Tipo { get; init; } = TipoMovimentacaoAnimal.Transferencia;

    [StringLength(150)]
    public string? LocalOrigem { get; init; }

    [StringLength(150)]
    public string? LocalDestino { get; init; }
    public Guid? AmbienteOrigemId { get; init; }
    public Guid? AmbienteDestinoId { get; init; }

    [StringLength(200)]
    public string? Motivo { get; init; }

    [StringLength(1000)]
    public string? Observacoes { get; init; }
}

public sealed record MovimentacaoAnimalResponseDto
{
    public Guid Id { get; init; }
    public Guid? GrupoId { get; init; }
    public Guid AnimalId { get; init; }
    public AnimalShortResponseDto? Animal { get; init; }
    public DateOnly DataMovimentacao { get; init; }
    public TipoMovimentacaoAnimal Tipo { get; init; }
    public string? LocalOrigem { get; init; }
    public string? LocalDestino { get; init; }
    public Guid? AmbienteOrigemId { get; init; }
    public Guid? AmbienteDestinoId { get; init; }
    public string? Motivo { get; init; }
    public string? Observacoes { get; init; }
    public DateTimeOffset CreationDateTime { get; init; }
}
