using Api.DTOs.Vacinas;
using Api.Models;

namespace Api.Mappers;

public static class VacinaMapper
{
    public static VacinaReadResponseDto MapToRead(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Name = vacina.Name,
        Descricao = vacina.Descricao,
        ReaplicarEmXDias = vacina.ReaplicarEmXDias,
        ObrigatorioOrgaoSanitario = vacina.ObrigatorioOrgaoSanitario,
        EspecieId = vacina.EspecieId,
        Especie = vacina.Especie != null ? EspecieMapper.MapToShort(vacina.Especie) : null
    };

    public static VacinaShortResponseDto MapToShort(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Name = vacina.Name,
        Descricao = vacina.Descricao
    };

    public static VacinaDetailResponseDto MapToResponse(Vacina vacina) => new()
    {
        Id = vacina.Id,
        Name = vacina.Name,
        Descricao = vacina.Descricao,
        ReaplicarEmXDias = vacina.ReaplicarEmXDias,
        ObrigatorioOrgaoSanitario = vacina.ObrigatorioOrgaoSanitario,
        EspecieId = vacina.EspecieId,
        Especie = vacina.Especie != null ? EspecieMapper.MapToHead(vacina.Especie) : null,
        TotalAplicacoes = (uint)vacina.Aplicacoes.Count,
        CreationDateTime = vacina.CreationDateTime,
        LastModificationDateTime = vacina.LastModificationDateTime
    };
}

