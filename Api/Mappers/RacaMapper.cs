using Api.DTOs.Especies;
using Api.DTOs.Racas;
using Api.Models;

namespace Api.Mappers;

public static class RacaMapper
{
    public static RacaReadDto MapToHead(Raca raca) => new()
    {
        Id = raca.Id,
        Nome = raca.Nome,
        EspecieId = raca.EspecieId
    };

    public static RacaResponseDto MapToResponse(Raca raca) => new()
    {
        Id = raca.Id,
        Nome = raca.Nome,
        Especie = new EspecieReadDto()
        {
            Id = raca.EspecieId,
            Nome = raca.Especie.Nome,
            NomeCientifico = raca.Especie.NomeCientifico
        },
    };
}