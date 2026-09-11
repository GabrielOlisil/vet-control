using Api.DTOs.Especies;
using Api.DTOs.Racas;
using Api.Models;

namespace Api.Mappers;

public static class RacaMapper
{
    public static RacaReadResponseDto MapToRead(Raca raca) => new()
    {
        Id = raca.Id,
        Nome = raca.Nome,
        EspecieId = raca.EspecieId,
        Especie = raca.Especie != null
            ? EspecieMapper.MapToShort(raca.Especie)
            : new EspecieShortResponseDto
            {
                Id = raca.EspecieId,
                Nome = string.Empty,
                FullName = string.Empty
            },
    };

    public static RacaShortResponseDto MapToShort(Raca raca) => new()
    {
        Id = raca.Id,
        Nome = raca.Nome,
    };

    public static RacaDetailResponseDto MapToResponse(Raca raca) => new()
    {
        Id = raca.Id,
        Nome = raca.Nome,
        EspecieId = raca.EspecieId,
        Especie = raca.Especie != null ? EspecieMapper.MapToHead(raca.Especie) : null,
        AnimalCount = (uint)raca.Animais.Count,
        CreationDateTime = raca.CreationDateTime,
        LastModificationDateTime = raca.LastModificationDateTime
    };
}