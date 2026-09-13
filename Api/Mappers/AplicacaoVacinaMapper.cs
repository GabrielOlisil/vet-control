using Api.DTOs;
using Api.Models;

namespace Api.Mappers;

public static class AplicacaoVacinaMapper
{
    public static AplicacaoVacinaReadResponseDto MapToHead(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        AnimalId = aplicacao.AnimalId,
        VacinaId = aplicacao.VacinaId,
        Animal = aplicacao.Animal != null ? AnimalMapper.MapToShort(aplicacao.Animal) : null,
        Vacina = aplicacao.Vacina != null
            ? VacinaMapper.MapToShort(aplicacao.Vacina)
            : new VacinaShortResponseDto { Id = aplicacao.VacinaId, Name = string.Empty },
        DataAplicacao = aplicacao.DataAplicacao,
        DataProximaDose = aplicacao.DataProximaDose,
        CicloFinalizado = aplicacao.CicloFinalizado,
        StatusComprovante = aplicacao.StatusComprovante,
        ComprovanteDocumentoPath = aplicacao.ComprovanteDocumentoPath,
        NumeroLote = aplicacao.NumeroLote,
        DoseMl = aplicacao.DoseMl,
    };

    public static AplicacaoVacinaShortResponseDto MapToShort(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        VacinaId = aplicacao.VacinaId,
        Vacina = aplicacao.Vacina != null ? VacinaMapper.MapToShort(aplicacao.Vacina) : null,
        DataAplicacao = aplicacao.DataAplicacao,
        NumeroLote = aplicacao.NumeroLote,
    };

    public static AplicacaoVacinaDetailResponseDto MapToResponse(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        AnimalId = aplicacao.AnimalId,
        VacinaId = aplicacao.VacinaId,
        Animal = aplicacao.Animal != null ? AnimalMapper.MapToHead(aplicacao.Animal) : null,
        Vacina = aplicacao.Vacina != null ? VacinaMapper.MapToRead(aplicacao.Vacina) : null,
        DataAplicacao = aplicacao.DataAplicacao,
        DataProximaDose = aplicacao.DataProximaDose,
        CicloFinalizado = aplicacao.CicloFinalizado,
        NumeroLote = aplicacao.NumeroLote,
        DoseMl = aplicacao.DoseMl,
        Observacoes = aplicacao.Observacoes,
        StatusComprovante = aplicacao.StatusComprovante,
        ComprovanteDocumentoPath = aplicacao.ComprovanteDocumentoPath,
        CreationDateTime = aplicacao.CreationDateTime,
        LastModificationDateTime = aplicacao.LastModificationDateTime
    };
}