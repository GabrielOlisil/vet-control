using Api.DTOs.Especies;
using Api.Models;

namespace Api.Mappers;

public static class EspecieMapper
{
    public static EspecieReadDto MapToHead(Especie especie) => new()
    {
        Id = especie.Id,
        Nome = especie.Nome,
        NomeCientifico = especie.NomeCientifico
    };

    public static EspecieResponseDto MapToResponse(Especie especie) => new()
    {
        Id = especie.Id,
        Nome = especie.Nome,
        NomeCientifico = especie.NomeCientifico,
        Racas =
        [
            .. especie.Racas.Select(raca => new RacaResumoDto
            {
                Id = raca.Id,
                Nome = raca.Nome
            })
        ]
    };
}
