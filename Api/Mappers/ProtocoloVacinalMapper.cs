using Api.DTOs;
using Api.Models;
using Api.Models.Enums;

namespace Api.Mappers;

public static class ProtocoloVacinalMapper
{
    public static ProtocoloVacinalShortResponseDto MapToShort(ProtocoloVacinal protocolo) => new()
    {
        FamiliaId = protocolo.FamiliaId, Versao = protocolo.Versao, VersaoAtual = protocolo.VersaoAtual,
        Id = protocolo.Id,
        Nome = protocolo.Nome,
        Finalidade = protocolo.Finalidade,
        Especie = protocolo.Especie is null ? null : EspecieMapper.MapToShort(protocolo.Especie)
    };

    public static ProtocoloVacinalDoseResponseDto MapDose(ProtocoloVacinalDose dose) => new()
    {
        Id = dose.Id,
        ProtocoloVacinalId = dose.ProtocoloVacinalId,
        VacinaId = dose.VacinaId,
        Ordem = dose.Ordem,
        DiasAposInicio = dose.DiasAposInicio,
        IntervaloReforcoDias = dose.IntervaloReforcoDias,
        Obrigatoria = dose.Obrigatoria,
        Observacoes = dose.Observacoes,
        Vacina = dose.Vacina is null ? null : VacinaMapper.MapToShort(dose.Vacina)
    };

    public static ProtocoloVacinalReadResponseDto MapToRead(ProtocoloVacinal protocolo) => new()
    {
        FamiliaId = protocolo.FamiliaId, Versao = protocolo.Versao, VersaoAtual = protocolo.VersaoAtual,
        Id = protocolo.Id,
        Nome = protocolo.Nome,
        Descricao = protocolo.Descricao,
        Finalidade = protocolo.Finalidade,
        EspecieId = protocolo.EspecieId,
        Especie = protocolo.Especie is null ? null : EspecieMapper.MapToShort(protocolo.Especie),
        Ativo = protocolo.Ativo,
        IdadeMinimaDias = protocolo.IdadeMinimaDias,
        IdadeMaximaDias = protocolo.IdadeMaximaDias,
        DoseCount = protocolo.Doses.Count
    };

    public static ProtocoloVacinalDetailResponseDto MapToDetail(ProtocoloVacinal protocolo) => new()
    {
        FamiliaId = protocolo.FamiliaId, Versao = protocolo.Versao, VersaoAtual = protocolo.VersaoAtual,
        Id = protocolo.Id,
        Nome = protocolo.Nome,
        Descricao = protocolo.Descricao,
        Finalidade = protocolo.Finalidade,
        EspecieId = protocolo.EspecieId,
        Especie = protocolo.Especie is null ? null : EspecieMapper.MapToShort(protocolo.Especie),
        Ativo = protocolo.Ativo,
        IdadeMinimaDias = protocolo.IdadeMinimaDias,
        IdadeMaximaDias = protocolo.IdadeMaximaDias,
        AnimalCount = protocolo.AplicacoesEmAnimais.Count,
        Doses = protocolo.Doses.OrderBy(d => d.Ordem).Select(MapDose).ToList(),
        CreationDateTime = protocolo.CreationDateTime,
        LastModificationDateTime = protocolo.LastModificationDateTime
    };

    public static AnimalProtocoloVacinalReadResponseDto MapAnimalProtocol(AnimalProtocoloVacinal assignment, DateOnly? referenceDate = null)
    {
        var doses = assignment.ProtocoloVacinal?.Doses
            .OrderBy(d => d.Ordem)
            .Select(dose =>
            {
                var application = assignment.Aplicacoes
                    .Where(a => a.ProtocoloVacinalDoseId == dose.Id && a.SituacaoRegistro == SituacaoRegistroVacina.Ativo)
                    .OrderByDescending(a => a.DataAplicacao)
                    .FirstOrDefault();
                var dataPrevista = assignment.DataInicio.AddDays(dose.DiasAposInicio);
                var status = StatusPendenciaVacinal.Pendente;

                if (application is not null)
                {
                    if (dose.IntervaloReforcoDias.HasValue)
                        dataPrevista = application.DataAplicacao.AddDays(dose.IntervaloReforcoDias.Value);
                    else
                        status = StatusPendenciaVacinal.Concluida;
                }

                if (status != StatusPendenciaVacinal.Concluida &&
                    dataPrevista < (referenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow)))
                    status = StatusPendenciaVacinal.Atrasada;

                return new AnimalProtocoloVacinalDoseResponseDto
                {
                    ProtocoloVacinalDoseId = dose.Id,
                    Ordem = dose.Ordem,
                    Vacina = dose.Vacina is null ? null : VacinaMapper.MapToShort(dose.Vacina),
                    DataPrevista = dataPrevista,
                    DataAplicacao = application?.DataAplicacao,
                    AplicacaoVacinaId = application?.Id,
                    Status = status,
                    Obrigatoria = dose.Obrigatoria
                };
            }).ToList() ?? [];

        return new AnimalProtocoloVacinalReadResponseDto
        {
            Id = assignment.Id,
            AnimalId = assignment.AnimalId,
            ProtocoloVacinalId = assignment.ProtocoloVacinalId,
            ProtocoloVacinal = assignment.ProtocoloVacinal is null ? null : MapToShort(assignment.ProtocoloVacinal),
            DataInicio = assignment.DataInicio,
            Ativo = assignment.Ativo,
            Observacoes = assignment.Observacoes,
            DosesConcluidas = doses.Count(d => d.Status == StatusPendenciaVacinal.Concluida),
            DosesPendentes = doses.Count(d => d.Status == StatusPendenciaVacinal.Pendente),
            DosesAtrasadas = doses.Count(d => d.Status == StatusPendenciaVacinal.Atrasada),
            Doses = doses,
            CreationDateTime = assignment.CreationDateTime,
            LastModificationDateTime = assignment.LastModificationDateTime
        };
    }
}
