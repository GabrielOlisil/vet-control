using Api.DTOs.Vacinas;
using Api.Models;

namespace Api.Mappers;

public static class VacinaMapper
{
    public static VacinaReadResponseDto MapToRead(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Name = vacina.Name,
        ReaplicarEmXDias = vacina.ReaplicarEmXDias
    };

    public static VacinaShortResponseDto MapToShort(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Name = vacina.Name,
    };
    public static VacinaDetailResponseDto MapToResponse(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Name = vacina.Name,
        ReaplicarEmXDias = vacina.ReaplicarEmXDias,
        CriadoEm = DateTime.UtcNow
    };
}
