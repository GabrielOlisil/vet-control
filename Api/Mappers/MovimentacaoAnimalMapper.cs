using Api.DTOs;
using Api.Models;

namespace Api.Mappers;

public static class MovimentacaoAnimalMapper
{
    public static MovimentacaoAnimalResponseDto MapToResponse(MovimentacaoAnimal movimentacao) => new()
    {
        Id = movimentacao.Id,
        GrupoId = movimentacao.GrupoId,
        AnimalId = movimentacao.AnimalId,
        Animal = movimentacao.Animal is null ? null : AnimalMapper.MapToShort(movimentacao.Animal),
        DataMovimentacao = movimentacao.DataMovimentacao,
        Tipo = movimentacao.Tipo,
        LocalOrigem = movimentacao.LocalOrigem,
        LocalDestino = movimentacao.LocalDestino,
        AmbienteOrigemId = movimentacao.AmbienteOrigemId,
        AmbienteDestinoId = movimentacao.AmbienteDestinoId,
        Motivo = movimentacao.Motivo,
        Observacoes = movimentacao.Observacoes,
        CreationDateTime = movimentacao.CreationDateTime
    };
}
