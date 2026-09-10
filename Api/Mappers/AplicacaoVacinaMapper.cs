using Api.DTOs.AplicacoesVacina;
using Api.DTOs.Vacinas;
using Api.Models;

namespace Api.Mappers;

public static class AplicacaoVacinaMapper
{
    public static AplicacaoVacinaReadResponseDto MapToHead(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        Vacina = new VacinaShortResponseDto { Id = aplicacao.VacinaId, Name = aplicacao.Vacina?.Name ?? string.Empty },
        AnimalId = aplicacao.AnimalId,
        DataAplicacao = aplicacao.DataAplicacao
    };

    public static AplicacaoVacinaDetailResponseDto MapToResponse(AplicacaoVacina aplicacao) => new()
    {
        Id = aplicacao.Id,
        AnimalId = aplicacao.AnimalId,
        VacinaId = aplicacao.VacinaId,
        Vacina = aplicacao.Vacina is null
            ? null
            : new VacinaDetailResponseDto
            {
                Id = aplicacao.Vacina.Id,
                Name = aplicacao.Vacina.Name,
                ReaplicarEmXDias = aplicacao.Vacina.ReaplicarEmXDias,
                CriadoEm = aplicacao.Vacina.CreationDateTime.DateTime
            },
        DataAplicacao = aplicacao.DataAplicacao,
        DataProximaDose = aplicacao.DataProximaDose,
        NumeroLote = aplicacao.NumeroLote,
        LaboratorioFabricante = null,
        DoseMl = aplicacao.DoseMl != null ? decimal.TryParse(aplicacao.DoseMl, out var d) ? d : null : null,
        VeterinarioResponsavel = string.Empty,
        Aplicador = null,
        Observacoes = aplicacao.Observacoes,
        StatusComprovante = Api.Models.Enums.StatusComprovanteVacina.NaoEmitido,
        TemComprovanteAnexo = false,
        CriadoEm = aplicacao.CreationDateTime,
        AtualizadoEm = aplicacao.LastModificationDateTime
    };
}