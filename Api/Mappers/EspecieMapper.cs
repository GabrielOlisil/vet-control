using Api.DTOs;
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
        RaceCount = (uint)especie.Racas.Count,
    };

    public static EspecieShortResponseDto MapToShort(Especie especie) => new()
    {
        Id = especie.Id,
        Nome = especie.Nome,
        FullName = string.IsNullOrWhiteSpace(especie.NomeCientifico)
            ? especie.Nome
            : string.Concat(especie.Nome, "  |  ", especie.NomeCientifico),
    };

    public static EspecieDetailResponseDto MapToResponse(Especie especie) => new()
    {
        Id = especie.Id,
        Nome = especie.Nome,
        NomeCientifico = especie.NomeCientifico,
        PortePadrao = especie.PortePadrao,
        IconeKey = especie.IconeKey,
        RaceCount = (uint)especie.Racas.Count,
        Racas = especie.Racas.Select(RacaMapper.MapToRead).ToList(),
        VacinasRestritas = especie.VacinasRestritas.Select(VacinaMapper.MapToRead).ToList(),
        CreationDateTime = especie.CreationDateTime,
        LastModificationDateTime = especie.LastModificationDateTime
    };
}

