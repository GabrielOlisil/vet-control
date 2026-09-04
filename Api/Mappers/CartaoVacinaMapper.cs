using Api.DTOs.AplicacoesVacina;
using Api.DTOs.CartoesVacina;
using Api.Models;

namespace Api.Mappers;

public static class CartaoVacinaMapper
{
    public static CartaoVacinaReadDto MapToHead(CartaoVacina cartao) => new()
    {
        Id = cartao.Id,
        VacinasAplicadas =
        [
            .. cartao.VacinasAplicadas
                .OrderByDescending(aplicacao => aplicacao.DataAplicacao)
                .Select(aplicacao => new AplicacaoVacinaResumoDto
                {
                    Id = aplicacao.Id,
                    VacinaId = aplicacao.VacinaId,
                    VacinaName = aplicacao.Vacina?.Name ?? string.Empty,
                    ReaplicarEmXDias = aplicacao.Vacina?.ReaplicarEmXDias ?? 0,
                    DataAplicacao = aplicacao.DataAplicacao
                })
        ]
    };

    public static CartaoVacinaResponseDto MapToResponse(CartaoVacina cartao) => new()
    {
        Id = cartao.Id,
        VacinasAplicadas =
        [
            .. cartao.VacinasAplicadas
                .OrderByDescending(aplicacao => aplicacao.DataAplicacao)
                .Select(aplicacao => new AplicacaoVacinaResponseDto
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
                })
        ]
    };
}
