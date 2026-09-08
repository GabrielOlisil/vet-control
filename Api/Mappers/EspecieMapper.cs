using Api.DTOs.Especies;
using Api.Models;

namespace Api.Mappers;

public static class EspecieMapper
{
    public static EspecieReadResponseDto MapToHead(Especie especie) => new()
    {
        Id = especie.Id,
        Nome = especie.Nome,
        NomeCientifico = especie.NomeCientifico,

    };

    public static EspecieDetailResponseDto MapToResponse(Especie especie) => new()
    {
        Id = especie.Id,
        Nome = especie.Nome,
        NomeCientifico = especie.NomeCientifico,
        RaceCount = (uint)especie.Racas.Count
    };
}
