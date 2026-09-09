using Api.DTOs.AplicacoesVacina;
using Api.DTOs.CartoesVacina;
using Api.DTOs.Vacinas;
using Api.Models;

namespace Api.Mappers;

public static class CartaoVacinaMapper
{
    public static CartaoVacinaReadResponseDto MapToHead(CartaoVacina cartao) => new()
    {
        Id = cartao.Id,
        VacinasAplicadas =
        [
            .. cartao.VacinasAplicadas
                .OrderByDescending(aplicacao => aplicacao.DataAplicacao)
                .Select(aplicacao => new AplicacaoVacinaShortResponseDto
                {
                    Id = aplicacao.Id,
                    Vacina = new VacinaShortResponseDto{Id = aplicacao.VacinaId, Name = aplicacao.Vacina?.Name ?? ""  },
                    DataAplicacao = aplicacao.DataAplicacao
                })
        ]
    };

    public static CartaoVacinaDetailResponseDto MapToResponse(CartaoVacina cartao) => new()
    {
        Id = cartao.Id,
        VacinasAplicadas =
        [
            .. cartao.VacinasAplicadas
                .OrderByDescending(aplicacao => aplicacao.DataAplicacao)
                .Select(aplicacao => new AplicacaoVacinaDetailResponseDto
                {
                    Id = aplicacao.Id,
                    VacinaName = aplicacao.Vacina?.Name ?? "",
                    CartaoVacinaId = aplicacao.CartaoVacinaId,
                    ReaplicarEmXDias = aplicacao.Vacina?.ReaplicarEmXDias ?? 0,
                    DataAplicacao = aplicacao.DataAplicacao
                })
        ]
    };
}
