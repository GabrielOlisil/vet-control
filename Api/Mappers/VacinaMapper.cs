using Api.DTOs.Vacinas;
using Api.Models;

namespace Api.Mappers;

public static class VacinaMapper
{
    public static VacinaReadDto MapToHead(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Name = vacina.Name,
        ReaplicarEmXDias = vacina.ReaplicarEmXDias
    };

    public static VacinaResponseDto MapToResponse(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Nome = vacina.Name,
        ReaplicarEmXDias = vacina.ReaplicarEmXDias,
        CriadoEm = DateTime.UtcNow
    };
}
