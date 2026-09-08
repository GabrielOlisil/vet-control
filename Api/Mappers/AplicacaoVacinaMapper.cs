using Api.DTOs.AplicacoesVacina;
using Api.DTOs.Vacinas;
using Api.Models;

namespace Api.Mappers;

public static class AplicacaoVacinaMapper
{
    public static AplicacaoVacinaReadResponseDto MapToHead(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        Vacina = new VacinaShortResponseDto { Id = aplicacao.VacinaId, Nome = aplicacao.Vacina?.Name ?? string.Empty },
        CartaoVacinaId = aplicacao.CartaoVacinaId,
        DataAplicacao = aplicacao.DataAplicacao
    };

    public static AplicacaoVacinaDetailResponseDto MapToResponse(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        VacinaName = aplicacao.Vacina?.Name ?? string.Empty,
        CartaoVacinaId = aplicacao.CartaoVacinaId,
        DataAplicacao = aplicacao.DataAplicacao
    };
}