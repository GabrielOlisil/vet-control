using Api.DTOs.Especies;
using Api.DTOs.Racas;
using Api.Models;

namespace Api.Mappers;

public static class RacaMapper
{
    public static RacaReadResponseDto MapToHead(Raca raca) => new()
    {
        Id = raca.Id,
        Nome = raca.Nome,
        Especie = new EspecieShortResponseDto
        {
            Id = raca.EspecieId,
            FullName = string.Concat([.. raca.Especie?.Nome ?? string.Empty,
            " ", raca.Especie?.NomeCientifico ?? string.Empty])
        },
    };

    public static RacaDetailResponseDto MapToResponse(Raca raca) => new()
    {
        Id = raca.Id,
        Nome = raca.Nome,
        Especie = new EspecieShortResponseDto()
        {
            Id = raca.EspecieId,
            FullName = string.Concat([.. raca.Especie?.Nome ?? string.Empty,
            " ", raca.Especie?.NomeCientifico ?? string.Empty])
        },
    };
}