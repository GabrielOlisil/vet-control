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
        AnimalProtocoloVacinalId = aplicacao.AnimalProtocoloVacinalId,
        ProtocoloVacinalDoseId = aplicacao.ProtocoloVacinalDoseId,
        Animal = aplicacao.Animal != null ? AnimalMapper.MapToShort(aplicacao.Animal) : null,
        Vacina = aplicacao.Vacina != null
            ? VacinaMapper.MapToShort(aplicacao.Vacina)
            : new VacinaShortResponseDto { Id = aplicacao.VacinaId, Name = string.Empty },
        DataAplicacao = aplicacao.DataAplicacao,
        DataProximaDose = aplicacao.DataProximaDose,
        CicloFinalizado = aplicacao.CicloFinalizado,
        StatusComprovante = aplicacao.StatusComprovante,
        NumeroLote = aplicacao.NumeroLote,
        DoseMl = aplicacao.DoseMl,
        DataFabricacao = aplicacao.DataFabricacao,
        DataValidade = aplicacao.DataValidade,
        LocalAplicacao = aplicacao.LocalAplicacao,
        VeterinarioResponsavel = aplicacao.VeterinarioResponsavel,
        VeterinarioCrmv = aplicacao.VeterinarioCrmv,
        VeterinarioCrmvUf = aplicacao.VeterinarioCrmvUf,
        Aplicador = aplicacao.Aplicador,
        LaboratorioFabricante = aplicacao.LaboratorioFabricante,
        AssinadaEm = aplicacao.AssinadaEm,
        Assinante = aplicacao.Assinante,
        AssinanteCrmv = aplicacao.AssinanteCrmv,
        AssinanteCrmvUf = aplicacao.AssinanteCrmvUf,
        HashAssinatura = aplicacao.HashAssinatura,
        MetodoAssinatura = aplicacao.MetodoAssinatura,
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
        AnimalProtocoloVacinalId = aplicacao.AnimalProtocoloVacinalId,
        ProtocoloVacinalDoseId = aplicacao.ProtocoloVacinalDoseId,
        Animal = aplicacao.Animal != null ? AnimalMapper.MapToHead(aplicacao.Animal) : null,
        Vacina = aplicacao.Vacina != null ? VacinaMapper.MapToRead(aplicacao.Vacina) : null,
        DataAplicacao = aplicacao.DataAplicacao,
        DataProximaDose = aplicacao.DataProximaDose,
        CicloFinalizado = aplicacao.CicloFinalizado,
        NumeroLote = aplicacao.NumeroLote,
        DoseMl = aplicacao.DoseMl,
        Observacoes = aplicacao.Observacoes,
        VeterinarioResponsavel = aplicacao.VeterinarioResponsavel,
        VeterinarioCrmv = aplicacao.VeterinarioCrmv,
        VeterinarioCrmvUf = aplicacao.VeterinarioCrmvUf,
        Aplicador = aplicacao.Aplicador,
        LaboratorioFabricante = aplicacao.LaboratorioFabricante,
        DataFabricacao = aplicacao.DataFabricacao,
        DataValidade = aplicacao.DataValidade,
        LocalAplicacao = aplicacao.LocalAplicacao,
        StatusComprovante = aplicacao.StatusComprovante,
        AssinadaEm = aplicacao.AssinadaEm,
        Assinante = aplicacao.Assinante,
        AssinanteCrmv = aplicacao.AssinanteCrmv,
        AssinanteCrmvUf = aplicacao.AssinanteCrmvUf,
        HashAssinatura = aplicacao.HashAssinatura,
        MetodoAssinatura = aplicacao.MetodoAssinatura,
        Assinaturas = aplicacao.Assinaturas
            .OrderByDescending(assinatura => assinatura.DataHora)
            .Select(assinatura => new AssinaturaAplicacaoVacinaResponseDto
            {
                Id = assinatura.Id,
                TipoEvento = assinatura.TipoEvento,
                NomeAssinante = assinatura.NomeAssinante,
                Crmv = assinatura.Crmv,
                CrmvUf = assinatura.CrmvUf,
                DataHora = assinatura.DataHora,
                HashConteudo = assinatura.HashConteudo,
                Metodo = assinatura.Metodo,
                Observacoes = assinatura.Observacoes
            })
            .ToList(),
        CreationDateTime = aplicacao.CreationDateTime,
        LastModificationDateTime = aplicacao.LastModificationDateTime
    };
}
