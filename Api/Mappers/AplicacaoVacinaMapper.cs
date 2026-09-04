using Api.DTOs.AplicacoesVacina;
using Api.Models;

namespace Api.Mappers;

public static class AplicacaoVacinaMapper
{
    public static AplicacaoVacinaReadDto MapToHead(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        VacinaId = aplicacao.VacinaId,
        VacinaName = aplicacao.Vacina?.Name ?? string.Empty,
        ReaplicarEmXDias = aplicacao.Vacina?.ReaplicarEmXDias ?? 0,
        CartaoVacinaId = aplicacao.CartaoVacinaId,
        DataAplicacao = aplicacao.DataAplicacao
    };

    public static AplicacaoVacinaResponseDto MapToResponse(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        Vacina = aplicacao.Vacina is null
            ? null!
            : new VacinaInfoDto
            {
                Id = aplicacao.Vacina.Id,
                Nome = aplicacao.Vacina.Name,
                ReaplicarEmXDias = aplicacao.Vacina.ReaplicarEmXDias
            },
        CartaoVacinaId = aplicacao.CartaoVacinaId,
        DataAplicacao = aplicacao.DataAplicacao
    };
}