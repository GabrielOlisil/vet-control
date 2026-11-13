using System.ComponentModel.DataAnnotations;
using Api.Models.Enums;

namespace Api.DTOs;

public sealed record AmbienteCreateDto
{
    [Required, StringLength(150)]
    public required string Nome { get; init; }
    public TipoAmbiente Tipo { get; init; }
}

public sealed record AmbienteResponseDto(Guid Id, string Nome, TipoAmbiente Tipo);
public sealed record AmbienteOcupacaoDto(Guid Id, string Nome, TipoAmbiente Tipo, int QuantidadeAnimais);
