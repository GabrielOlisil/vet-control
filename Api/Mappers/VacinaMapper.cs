using Api.DTOs;
using Api.Models;

namespace Api.Mappers;

public static class VacinaMapper
{
    public static VacinaReadResponseDto MapToRead(Vacina vacina) => new()
    {
        Ativo = vacina.Ativo,
        Id = vacina.Id,
        Name = vacina.Name,
        Descricao = vacina.Descricao,
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
        Ativo = vacina.Ativo,
        Id = vacina.Id,
        Name = vacina.Name,
        Descricao = vacina.Descricao,
        ObrigatorioOrgaoSanitario = vacina.ObrigatorioOrgaoSanitario,
        EspecieId = vacina.EspecieId,
        Especie = vacina.Especie != null ? EspecieMapper.MapToHead(vacina.Especie) : null,
        TotalAplicacoes = (uint)vacina.Aplicacoes.Count(a => a.SituacaoRegistro == Api.Models.Enums.SituacaoRegistroVacina.Ativo),
        CreationDateTime = vacina.CreationDateTime,
        LastModificationDateTime = vacina.LastModificationDateTime
    };
}

