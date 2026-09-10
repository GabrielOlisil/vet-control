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
        PortePadrao = especie.PortePadrao,
        IconeKey = especie.IconeKey,
    };

    public static EspecieShortResponseDto MapToShort(Especie especie) => new()
    {
        Id = especie.Id,
        FullName = string.Concat(especie.Nome, "  |  ", especie.NomeCientifico),
    };

    public static EspecieDetailResponseDto MapToResponse(Especie especie) => new()
    {
        Id = especie.Id,
        Nome = especie.Nome,
        NomeCientifico = especie.NomeCientifico,
        PortePadrao = especie.PortePadrao,
        IconeKey = especie.IconeKey,
        RaceCount = (uint)especie.Racas.Count
    };
}
